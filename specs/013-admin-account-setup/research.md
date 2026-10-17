# Research: Admin account setup

**Feature:** 013-admin-account-setup
**Plan:** [plan.md](plan.md)

Each decision lists the options, the choice and why. None of them needs a package outside the "Allowed dependencies" table of `docs/architecture.md` v1.1.0.

## R1. Where the startup setup runs

FR-002 and FR-017 require the setup to finish (or refuse) before the system listens on any port or answers any request, and a refused start must end the process with a non-zero exit code.

| Option | Pros | Cons |
|--------|------|------|
| A. Explicit step in `Program.cs`, between `builder.Build()` and `app.RunAsync()` | Runs strictly before Kestrel starts. A refusal is a plain `return 1`, with no exception. Easy to read in the composition root. `WebApplicationFactory` runs it too, because it resolves the host with `stopApplication: false` and the entry point keeps running until its own `app.RunAsync()`. | `Program.cs` grows by about 50 lines. The refusal branch is only covered by process tests, not by in-process coverage. |
| B. `IHostedService.StartAsync` | Self-contained in Infrastructure. | Order against the web server depends on registration order. A refusal can only be signalled by throwing, which logs a host-level stack trace and uses an exception for an expected failure (architecture "Errors"). |
| C. `IHostedLifecycleService.StartingAsync` | Guaranteed to run before the server starts. | Same problem as B: a refusal must throw. |

**Choice: A.** The step creates a scope, resolves `SetUpAdminAccountCommand` and `SetUpAdminAccountHandler`, runs the handler, logs the outcome and either continues to `app.RunAsync()` or disposes the app and returns 1. Disposing the app disposes the console logger provider, which flushes its queue, so every problem is written (FR-017).

Expected failures come back as a `Result` failure and get their own message: invalid setup values, missing connection string, migration lock timeout, more than one Admin account, unusable stored password. Only a truly unexpected exception (for example, a file that is not a SQLite database) reaches the single `catch (Exception)` in `Program.cs`. That catch logs it at `Critical` with the exception and no setup value (constitution P9, P7), and the process returns 1 (FR-014). Nothing is swallowed.

A refused start cannot be tested in-process: when the entry point returns before `RunAsync`, the factory's start succeeds on a disposed host. Refusals are tested only as process tests (R8), and `ShopApiFactory` always supplies valid setup values.

## R2. Reading the setup values only from the environment (FR-003)

`WebApplication.CreateBuilder` merges these sources into one `IConfiguration`, so a value in `appsettings.json` would be read unless it is excluded:
- `appsettings.json` and `appsettings.{Environment}.json`
- user secrets (Development only)
- environment variables
- command-line arguments

| Option | Pros | Cons |
|--------|------|------|
| A. Separate `ConfigurationBuilder` in `Program.cs` with only `AddUserSecrets<Program>(optional: true)` (Development only) and `AddEnvironmentVariables()` | The delivered configuration can never feed a setup value (US2 #7). Uses only types from the ASP.NET Core shared framework that the Api already has. Matches architecture "Configuration: bound in Program.cs; secrets from user-secrets or environment variables". | Two configuration objects in `Program.cs`. |
| B. Filter the providers of `builder.Configuration` by type | One configuration object. | Needs the provider types in Infrastructure, whose packages are not on the allowlist. Telling the user-secrets JSON provider apart from `appsettings.json` would rely on its file name. |
| C. Read the values from `builder.Configuration` and check in a test that `appsettings*.json` has no `AdminSetup` section | Simplest. | Does not meet FR-003 at runtime: a value placed in the delivered configuration after release would be used. |

**Choice: A.** `Program.cs` binds the two keys into `SetUpAdminAccountCommand` (Application, following the `<Action>Command` convention) and registers it as a singleton. The startup step resolves the command and passes it to the handler, and tests replace the registration through `ConfigureTestServices`.

The command is a `sealed class`, not a record. A record's generated `ToString()` would print the initial password, and any accidental log, assertion message or exception text holding the command would leak it (FR-008, P7). Its `ToString()` returns only the type name, and a unit test checks this.

Command-line arguments are not a source, so there is no setup command (FR-003).

Setting names are `AdminSetup:SignInName` and `AdminSetup:InitialPassword` (environment variables `AdminSetup__SignInName` and `AdminSetup__InitialPassword`). Spec 019 adds its own keys under the same `AdminSetup` section.

## R3. Password storage format

| Option | Pros | Cons |
|--------|------|------|
| A. Four columns: algorithm (text), salt (blob), iteration count (integer), hash (blob) | Each P4 property is checked directly (FR-014). Tests read each value. No parsing. | Four columns instead of one. |
| B. One encoded string (`$pbkdf2-sha256$600000$<salt>$<hash>`) | Common format, one column. | Needs a parser and its error handling to decide "unusable"; a malformed string is one more failure path. |

**Choice: A**, held in the Domain value object `StoredPassword`.

Parameters:
- PBKDF2 with HMAC-SHA256, using the static `Rfc2898DeriveBytes.Pbkdf2`. The constructors are obsolete and would fail warnings-as-errors.
- A 16-byte salt from `RandomNumberGenerator`.
- 600,000 iterations.
- A 32-byte hash.
- The password bytes are the UTF-8 encoding of the NFC-normalized password (R5). The byte buffer is cleared with `CryptographicOperations.ZeroMemory` after use.

`StoredPassword.IsUsable` is false when:
- any field is missing;
- the algorithm is not `PBKDF2-HMAC-SHA256`;
- the salt is shorter than 16 bytes;
- the iteration count is below 600,000;
- the hash is not 32 bytes long.

The first four follow FR-014's list directly. The hash-length check is an interpretation of FR-014's "unreadable": PBKDF2-HMAC-SHA256 here always yields 32 bytes, so a hash of any other length cannot be compared.

Checking an entered password (with the constant-time comparison) belongs to 014. This feature adds only `Hash`; 014 adds `Verify` to the same `IPasswordHasher`. Tests here recompute PBKDF2 directly with the stored salt and iteration count.

### Reading a damaged password record (FR-014, 019 hook)

FR-014 counts a password record that is "missing or unreadable" as unusable, and requires that this feature not refuse before spec 019 can apply a recovery. So a damaged record must come back through the `Result` path (`UnusableStoredPassword`), never through the generic exception catch. The persistence model therefore separates the stored row from the Domain object:
- Infrastructure maps `AdminAccountRecord` for writing and for the schema. Its columns are created `NOT NULL` (`IsRequired()`), so the system itself never writes NULL.
- EF Core reads an `IsRequired()` column with a non-null reader. A NULL value then throws `InvalidOperationException` ("The data is NULL at ordinal N"), even when the C# property is nullable or the query casts it to a nullable type (`Select` with `(string?)`, `EF.Property<T?>`). Both were tried during 013's build. So reading goes through a second, keyless type, `AdminAccountRow`, mapped to the same table with `ToView("AdminAccounts")`. Its properties are nullable and not required. `ToView` keeps it out of migrations, so the schema and the `AddAdminAccounts` migration do not change. Reading stays in the ORM, with no raw SQL.
  - Rejected: removing `IsRequired()`, which would drop NOT NULL from the schema; a constant `Database.SqlQuery<T>`, which would bring raw SQL into production code.
- The repository converts each row into a Domain `AdminAccount` through `AdminAccount.Restore(...)`. A NULL password field becomes an empty value, so `IsUsable` is false. SQLite converts values of the wrong type when they are read (text read as an integer gives 0, text read as a blob gives its bytes), so those also end up unusable rather than throwing.
- A NULL sign-in name is not a password problem. The repository returns it as a failed `Result` (`UnreadableStoredData`), and the start is refused with no recovery. FR-014's exception covers only the password.

## R4. Exactly one Admin account, also under concurrent starts (FR-001, FR-012)

| Option | Pros | Cons |
|--------|------|------|
| A. Fixed primary key `Id = 1` plus a `CHECK ("Id" = 1)` constraint | The database itself refuses a second row. A concurrent loser gets a primary-key violation and simply re-reads. One INSERT is atomic, so an interrupted creation leaves no partial row (FR-012). | The loser must recognise the violation. |
| B. Application-level check only | No constraint. | Two simultaneous starts can both insert. |
| C. Explicit lock (file lock or `BEGIN EXCLUSIVE`) around check-and-insert | Also works. | More code; still needs A to protect against other writers. |

**Choice: A.**
- `AdminAccountRepository.AddAsync` catches only a `DbUpdateException` whose inner `SqliteException` has `SqliteExtendedErrorCode` 1555 (`SQLITE_CONSTRAINT_PRIMARYKEY`, which a duplicate `INTEGER PRIMARY KEY` raises).
- It detaches the record and returns `AddAdminAccountOutcome.AlreadyExists`. Any other exception is rethrown.
- The handler then re-reads once and continues as in US3, with no creation log.
- FR-014's "more than one Admin account" check stays in the handler (count > 1), for stored data changed outside the system.

EF Core logs this expected violation at Error level (R7). Those log categories are silenced so the race path leaves no trace of the parameters.

## R5. Unicode rules (FR-006, FR-007, FR-015)

- **Normalization:** `string.Normalize(NormalizationForm.FormC)` runs before any check, hash or store.
- **Length:** count Unicode scalar values with `value.EnumerateRunes().Count()`, so an emoji or an accented letter counts as 1.
- **Trimming the sign-in name:** `string.Trim()` with no arguments. It removes every `char.IsWhiteSpace` character (the Unicode White_Space set: space, tab, line breaks, non-breaking space…). The password is never trimmed.
- **Control characters:** `Rune.IsControl` (Unicode category Cc), the same definition spec 019 FR-003 uses. Format characters (Cf, such as a zero-width space) are allowed, because the spec only refuses control characters.
- **Missing vs empty:**
  - A setting that is absent or `""` is missing.
  - A sign-in name made only of whitespace is "empty" (US2 #5).
  - A password made only of spaces is a normal password (edge case).
- **Where the rules live:** in the Domain (`AdminCredentialRules`), so 014, 018 and 019 reuse the same normalization and counting (FR-015).

### Text that cannot be normalized (interpretation; needs the user's confirmation)

`Normalize` and `IsNormalized` throw `ArgumentException` on an unpaired UTF-16 surrogate. Such a value can reach the system only through a Windows environment variable; on Linux, environment variables are decoded from UTF-8 and never hold one. FR-015 cannot be applied to such a value, so it cannot be accepted. FR-004 says an invalid setup value refuses the start.

Without explicit handling, the exception would reach the generic catch and be logged with a misleading message. So:
- `AdminCredentialRules.Normalize*` catches only `ArgumentException` and returns `null`.
- The validator reports `null` as the rule "is not valid Unicode text", and the start is refused like any other invalid value.

This is the plan's reading of FR-004 + FR-015 rather than a rule written in the spec. It is listed in the plan under "Interpretations to confirm". If the user wants it written into the spec (it also applies to 014, 018 and 019), that goes through `/sdd-amend spec`.

## R6. Creating and upgrading the stored data (FR-016)

| Option | Pros | Cons |
|--------|------|------|
| A. EF Core code-first migrations applied with `Database.MigrateAsync()` at start | Creates the SQLite file when it is missing and applies any newer migration, with no step for the system operator (SC-004). Matches architecture "Persistence" (migrations in `Infrastructure/Persistence/Migrations/`). | Needs the `dotnet-ef` local tool to generate migrations (allowed since architecture 1.1.0). EF Core 9+ takes a migration lock on SQLite that a killed process leaves behind (below). |
| B. `EnsureCreated()` | No tool. | Cannot upgrade an existing database; breaks FR-016 as soon as a second migration exists. |

**Choice: A.** Application declares `IDatabaseMigrator`; Infrastructure's `DatabaseMigrator` calls `MigrateAsync`, and the handler calls it first, before reading the Admin account (FR-016). This is the first migration in the project (`AddAdminAccounts`).

The local tool manifest `.config/dotnet-tools.json` is created at the repository root. The commands run from `src/backend`, so the SDK pinned in `src/backend/global.json` applies, and the manifest is still found by walking up:
- `dotnet new tool-manifest --output ../..`
- `dotnet tool install dotnet-ef --version 10.0.13` (matching EF Core)
- `dotnet tool install dotnet-reportgenerator-globaltool`

**Stale migration lock.** On SQLite, EF Core 9+ serializes migrations with a row in `__EFMigrationsLock`. It retries until it gets the lock, and the row is not released if the process is killed while holding it. Then every later start would wait forever, never listening and never exiting, which breaks FR-017 and the FR-012 edge case. So:
- `DatabaseMigrator` passes `MigrateAsync` a token that cancels after a timeout (60 s by default, a constructor parameter so a test can use 2 s).
- It catches only the `OperationCanceledException` caused by that timeout, and returns `Result.Failure(StoredDataLocked)`.
- The start is refused with a message, without any value, that names the lock and how to clear it.
- The build task confirms that EF Core 10.0.13's SQLite lock wait honors the cancellation token. If it does not, the task stops and reports back.

**Connection string.** `ConnectionStrings:Default` is a connection string, so constitution P7 keeps it out of source and committed configuration. It comes from any configuration source that is not committed: environment variables, user secrets in Development, or command-line arguments. A test checks that no committed `appsettings*.json` contains a `ConnectionStrings` section.

It is read when the `AppDbContext` is first resolved, so test overrides apply. `DatabaseMigrator` checks it first: when it is absent or empty, it returns `Result.Failure(ConnectionStringMissing)`, and the start is refused with "ConnectionStrings:Default is missing" (P6: configuration is validated at the boundary). It never reaches the generic exception path.

SC-004 counts the steps to set up the *Admin account*. Where the system keeps its data is a deployment setting that every feature needs, and the constitution, which outranks the spec, forbids a committed default for it. The plan reads SC-004 accordingly (see "Interpretations to confirm" in the plan).

## R7. Logging without values (FR-005, FR-011, FR-013, NFR-003)

- The Application layer has no logging package (only `Microsoft.Extensions.DependencyInjection.Abstractions` is allowed there). The handler returns what happened, and `Program.cs` logs it with `app.Logger`. Every message is a fixed text plus setting names and rules, never a value or a length.
- The creation log includes the creation time from `TimeProvider` (`SetUpAdminAccountOutcome.CreatedAtUtc`), so it does not depend on the console formatter's timestamp option (FR-013).
- EF Core logs each SQL command with its parameter facets (`@p0='?' (Size = 5)`), which would reveal the sign-in name's length. On a failed command, `RelationalEventId.CommandError` and `CoreEventId.SaveChangesFailed` are logged at Error level with the same facets, so a `Warning` filter alone is not enough (the expected primary-key race of R4 would log them). `appsettings.json` therefore sets:
  - `Microsoft.EntityFrameworkCore`: `Warning`
  - `Microsoft.EntityFrameworkCore.Database.Command`: `None`
  - `Microsoft.EntityFrameworkCore.Update`: `None`

  Unexpected database exceptions are still logged once, by the `Program.cs` catch (P9). `EnableSensitiveDataLogging` is never used.
- Refusals are logged at `Critical`, the initial-password warning at `Warning`, and the creation at `Information`.

## R8. Testing a refused start

A refused start ends the process before the host starts. `WebApplicationFactory` cannot observe the exit code or the "not listening" property.

| Option | Pros | Cons |
|--------|------|------|
| A. Process tests: run the built `ShopManagement.Api.dll` with `dotnet`, set the environment, capture stdout/stderr and the exit code | Tests the real contract of FR-017 and FR-003 (real environment variables, real `appsettings.json`). No new package (`System.Diagnostics.Process`). | Slower (a few seconds each); limited to the scenarios that need it. |
| B. Only unit tests of the handler | Fast. | Leaves FR-017, FR-003 and the log content untested end to end. |

**Choice: A for a small set**: refusals, values only in the delivered configuration, damaged stored data, and the creation log. The handler gets unit tests for every rule permutation. In-process `WebApplicationFactory` tests with a capturing logger provider cover the starts that succeed (US1, US3, US4).

How the process tests run:
- **Locating the Api.** `RepositoryRoot` walks up from the test assembly to the folder holding `ShopManagement.slnx`. The tests use the Api's own build output there (`src/ShopManagement.Api/bin/<Configuration>/net10.0/`), with the same configuration as the test assembly, as the working directory. If the dll is missing, they fail with a clear message.
- **Environment.** The child process inherits the parent's environment, with these removed: every `AdminSetup__*`, `ConnectionStrings__*`, `ASPNETCORE_*`, `DOTNET_ENVIRONMENT` and `Logging__*` variable. `PATH`, `SystemRoot`, `USERPROFILE` and `DOTNET_ROOT` stay, because `dotnet` needs them. The test then sets:
  - `ASPNETCORE_ENVIRONMENT=Production`
  - `ASPNETCORE_URLS=http://127.0.0.1:0`
  - `ConnectionStrings__Default`, pointing at a temporary SQLite file
  - its own `AdminSetup__*` values
- **Refused start.** Wait for exit (timeout 60 s), then check that the exit code is not 0 and that "Now listening on" never appears.
- **Successful start.** Wait for "Now listening on", then kill the process.
- **US2 #7.** `ASPNETCORE_CONTENTROOT` points at a temporary folder whose `appsettings.json` holds `AdminSetup` values.
- **Test values (NFR-003, SC-001).** The sign-in name and initial password are built at runtime from a fixed prefix plus random characters, for example `op-` + 8 random letters and digits, and `Kx9!` + 12 random characters. So no literal test password is committed, and SC-001's search of committed files stays meaningful. The output is searched for both values, for the stored hash in base64 and hex, and for length phrases ("11 characters", "length").

## R9. Measuring backend coverage (constitution P3)

The test command does not measure coverage yet. Architecture 1.1.0 allows `coverlet.collector` (already referenced) and `dotnet-reportgenerator-globaltool` (local tool).

**Choice.** The "Run tests" line in `CLAUDE.md` runs in bash (the project's commands run in Git Bash on this machine) and becomes:

```
rm -rf src/backend/TestResults && dotnet test src/backend --collect:"XPlat Code Coverage" --results-directory src/backend/TestResults && dotnet reportgenerator -reports:"src/backend/TestResults/**/coverage.cobertura.xml" -targetdir:src/backend/TestResults/coverage -reporttypes:TextSummary -classfilters:"-ShopManagement.Infrastructure.Persistence.Migrations.*;-ShopManagement.Infrastructure.Persistence.DesignTimeDbContextFactory" -minimumCoverageThresholds:lineCoverage=80 && npm --prefix src/frontend test
```

ReportGenerator exits non-zero below 80% line coverage. Deleting the results folder first stops old reports from being merged, and the folder is git-ignored. Generated migrations and the design-time factory (used only by `dotnet ef`) are excluded.

Frontend coverage is not set up here, because this feature does not change the frontend. The first feature that does (014) adds `@vitest/coverage-v8` with its 80% threshold.

## R10. Integration test database

- **Connection string.** Each test uses a named shared-cache in-memory SQLite database, in the documented Microsoft.Data.Sqlite form `Data Source=<guid>;Mode=Memory;Cache=Shared`. EF Core's SQLite database creator recognizes `Mode=Memory`.
- **Sentinel connection.** The in-memory database lives as long as one connection to it stays open. That sentinel connection is owned by the **test class fixture**, not by the factory, so it outlives every factory of the test. Each `AppDbContext` opens its own connection.
- **Restart.** Two factories created one after the other with the same database name simulate a restart (US3).
- **Concurrent starts (FR-001).** This test uses a temporary SQLite **file**, because two processes share a file, not a shared cache. It runs the migrations once first, then runs the two handler calls in parallel, so the race is only on the insert.
