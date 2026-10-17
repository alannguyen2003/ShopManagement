# Tasks: Admin account setup

**Feature:** 013-admin-account-setup
**Plan:** [plan.md](plan.md)
**Status:** Approved
**Approved:** 2026-10-17
**Revision:** 2
**Plan revision:** 2
**Created:** 2026-10-17

Legend: `[P]` can run in parallel · `[USn]` belongs to user story n · tests always come before code.

Paths are relative to `src/backend/` unless they start with `.config/`, `.gitignore` or `CLAUDE.md` (repository root). `Domain/…`, `Application/…`, `Infrastructure/…`, `Api/…` mean `src/ShopManagement.<Layer>/…`; `Domain.Tests/…`, `Application.Tests/…`, `IntegrationTests/…` mean `tests/ShopManagement.Domain.Tests/…`, `tests/ShopManagement.Application.Tests/…`, `tests/ShopManagement.Api.IntegrationTests/…`.

Test values for log and storage searches always come from `TestSecrets` (generated at runtime, research R8); no test commits a literal password.

## Phase 1 — Setup

- [x] T001 Create the local tool manifest at the repository root and install `dotnet-ef` 10.0.13 and `dotnet-reportgenerator-globaltool` into it (from `src/backend`: `dotnet new tool-manifest --output ../..`, `dotnet tool install dotnet-ef --version 10.0.13`, `dotnet tool install dotnet-reportgenerator-globaltool`) — .config/dotnet-tools.json
- [x] T002 [P] Add a `UserSecretsId` to the Api with `dotnet user-secrets init --project src/ShopManagement.Api` (from `src/backend`) — Api/ShopManagement.Api.csproj
- [x] T003 [P] Ignore SQLite files and backend test results: `*.db`, `*.db-shm`, `*.db-wal`, `src/backend/TestResults/` — .gitignore

**Checkpoint:** from `src/backend`, `dotnet tool restore` succeeds, `dotnet ef --version` prints 10.0.13 and `dotnet build` passes with 0 warnings.

## Phase 2 — Foundation (blocks all later phases)

A test-writing task leaves its test project not compiling until its implementation task is done, so tasks in the same test project are not marked `[P]` here.

Domain

- [x] T004 Write tests for `Error.FieldErrors`: it defaults to `null`; it keeps a setting → rule-texts map; `Error.None` and the existing `Result` tests are unchanged — Domain.Tests/Common/ResultTests.cs
- [x] T005 Add the optional `FieldErrors` parameter to `Error` (contracts/application.md) so T004 passes — Domain/Common/Result.cs
- [x] T006 Write tests for `AdminCredentialRules` — Domain.Tests/AdminAccounts/AdminCredentialRulesTests.cs:
  - `NormalizeSignInName`: NFC; trims space, tab, line break and U+00A0 at both ends; keeps letter case ("  Shop Admin  " → "Shop Admin", "admin\n" → "admin", " admin" → "admin").
  - `NormalizePassword`: NFC only; leading and trailing spaces kept; NFD "café-shop-2026" (15 code points) → NFC form (14 code points).
  - Both return `null` for a value with an unpaired UTF-16 surrogate, without throwing (research R5).
  - `ValidateSignInName`: 1 and 100 code points valid; 0 → `Empty`; 101 → `TooLong`; "shop\tadmin" → `ControlCharacter`; "owner" valid (no email check).
  - `ValidatePassword`: 12 and 128 code points valid; 11 → `TooShort`; 129 → `TooLong`; line break or tab → `ControlCharacter`; 12 spaces valid; 11 single-code-point emoji (e.g. U+1F600) + 1 letter valid; a Cf character (U+200B) valid.
- [x] T007 Implement `CredentialRuleViolation` and `AdminCredentialRules` (NFC, `Trim()`, `EnumerateRunes().Count()`, `Rune.IsControl`; `Normalize*` catches only `ArgumentException`) so T006 passes — Domain/AdminAccounts/CredentialRuleViolation.cs, Domain/AdminAccounts/AdminCredentialRules.cs
- [x] T008 Write tests for `StoredPassword.IsUsable`: true for `PBKDF2-HMAC-SHA256`, 16-byte salt, 600,000 iterations, 32-byte hash; false for another or empty algorithm, a 15-byte or empty salt, 599,999 iterations, a hash of 31, 33 or 0 bytes — Domain.Tests/AdminAccounts/StoredPasswordTests.cs
- [x] T009 Implement `StoredPassword` with its constants (data-model.md) so T008 passes — Domain/AdminAccounts/StoredPassword.cs
- [x] T010 Write tests for `AdminAccount`: `Create` gives `Id == SingleId == 1` and keeps the name and password; `Create` throws `ArgumentException` for a name that breaks `AdminCredentialRules` or an unusable password; `Restore` accepts both without a check — Domain.Tests/AdminAccounts/AdminAccountTests.cs
- [x] T011 Implement `AdminAccount` (`Create`, `Restore`) so T010 passes — Domain/AdminAccounts/AdminAccount.cs

Application contracts

- [x] T012 [P] Write tests for `SetUpAdminAccountCommand`: `ToString()` contains neither the sign-in name nor the initial password; `SignInNameSetting == "AdminSetup:SignInName"` and `InitialPasswordSetting == "AdminSetup:InitialPassword"` — Application.Tests/AdminAccounts/SetUpAdminAccount/SetUpAdminAccountCommandTests.cs
- [x] T013 Implement `SetUpAdminAccountCommand` as a sealed class whose `ToString()` returns only the type name (research R2) so T012 passes — Application/AdminAccounts/SetUpAdminAccount/SetUpAdminAccountCommand.cs
- [x] T014 Declare the Application interfaces and enum of contracts/application.md, with no logic: `IDatabaseMigrator` (Application/Common/), `IPasswordHasher`, `IAdminAccountRepository`, `AddAdminAccountOutcome` (Application/AdminAccounts/). Their use is tested by T020, T022 and T028. The error classes and outcome types are created by the tasks whose tests assert them (T021, T023, T029, T041, T047) — Application/Common/IDatabaseMigrator.cs, Application/AdminAccounts/IPasswordHasher.cs, IAdminAccountRepository.cs, AddAdminAccountOutcome.cs

Infrastructure (tested from IntegrationTests, as there is no Infrastructure test project)

- [x] T015 Write tests for `Pbkdf2PasswordHasher` — IntegrationTests/AdminAccounts/Pbkdf2PasswordHasherTests.cs:
  - The result has algorithm `PBKDF2-HMAC-SHA256`, a 16-byte salt, 600,000 iterations and a 32-byte hash (NFR-001).
  - `Rfc2898DeriveBytes.Pbkdf2` over the UTF-8 bytes, with the stored salt and iterations and SHA-256, recomputes the stored hash (P4).
  - 10 hashes of the same password give 10 distinct salts and 10 distinct hashes (NFR-002).
  - The hash of "  my shop pass  " is not reproduced by recomputing "my shop pass" with the same salt (US1 #8).
- [x] T016 Implement `Pbkdf2PasswordHasher` (static `Rfc2898DeriveBytes.Pbkdf2`, salt from `RandomNumberGenerator`, password bytes cleared with `CryptographicOperations.ZeroMemory`) so T015 passes — Infrastructure/Security/Pbkdf2PasswordHasher.cs
- [x] T017 Write schema tests against a migrated in-memory SQLite database: inserting `Id = 2` fails on `CK_AdminAccounts_SingleRow`; a second row with `Id = 1` fails with extended code 1555; inserting NULL into each of the five non-key columns fails (`Id` is the rowid, so SQLite replaces a NULL there) (FR-001) — IntegrationTests/AdminAccounts/AdminAccountSchemaTests.cs
- [x] T018 (redo — plan rev 2) Implement per data-model.md — Infrastructure/Persistence/AdminAccountRecord.cs, AppDbContext.cs, DesignTimeDbContextFactory.cs, Configurations/AdminAccountRecordConfiguration.cs:
  - `AdminAccountRecord` is the write model. Its properties are non-nullable `string`, `byte[]` and `int`, matching the `IsRequired()` columns. They are declared `required`, so CS8618 is satisfied without `null!`.
  - `AppDbContext` has `DbSet<AdminAccountRecord> AdminAccounts`.
  - `AdminAccountRecordConfiguration`: table `AdminAccounts`, `ValueGeneratedNever`, `IsRequired()` on every column, `HasCheckConstraint("CK_AdminAccounts_SingleRow", "\"Id\" = 1")`.
  - `DesignTimeDbContextFactory` (`Data Source=:memory:`, no secret) stays unchanged.
  - Done when `dotnet build src/ShopManagement.Infrastructure` (from `src/backend`) has 0 warnings.
  - The uncommitted `AdminAccountRepository` and `StoredDataErrors` drafts from the first T021 attempt belong to T021. Keep them out of the T018 commit, and set the draft repository aside if it no longer compiles. T017 and T020 run again in T021.
- [x] T019 Generate the first migration so T017 passes: from `src/backend`, `dotnet ef migrations add AddAdminAccounts --project src/ShopManagement.Infrastructure --startup-project src/ShopManagement.Api --output-dir Persistence/Migrations` — Infrastructure/Persistence/Migrations/
- [x] T020 Write tests for `AdminAccountRepository` — IntegrationTests/AdminAccounts/AdminAccountRepositoryTests.cs:
  - `ListAsync` on an empty table → success with no account.
  - `AddAsync` → `Added`; `ListAsync` then returns the name and the four password fields. A `SaveChangesInterceptor` on the test context counts exactly one `SaveChangesAsync` call for the insert (FR-012).
  - `AddAsync` when the row exists → `AlreadyExists`, stored row unchanged (research R4).
  - Password columns NULL (the test rebuilds the table without NOT NULL) → an account whose password is not usable, no exception.
  - `PasswordIterations` stored as text → an account whose password is not usable.
  - `SignInName` NULL → failure `StoredDataErrors.UnreadableStoredData`, code `StoredData.Unreadable` (FR-014).
- [x] T021 Make T020 pass — Application/Common/StoredDataErrors.cs, Infrastructure/Persistence/AdminAccountRow.cs, Configurations/AdminAccountRowConfiguration.cs, AppDbContext.cs, Repositories/AdminAccountRepository.cs:
  - Create `StoredDataErrors` with `UnreadableStoredData`.
  - Add the keyless read model `AdminAccountRow`: `Id` is `int`, and the other five properties are nullable and not required; `HasNoKey()` and `ToView("AdminAccounts")` in `AdminAccountRowConfiguration`. Expose it as `AppDbContext.AdminAccountRows` (data-model.md, research R3).
  - Implement `AdminAccountRepository`:
    - `ListAsync` reads `AdminAccountRows` and converts each row with `AdminAccount.Restore`. A NULL password field becomes an empty value or 0; a NULL name gives `UnreadableStoredData`.
    - `AddAsync` writes an `AdminAccountRecord`. It catches only a `DbUpdateException` whose `SqliteException` has extended code 1555, detaches the record and returns `AlreadyExists`, and rethrows anything else.
  - Then run the schema (T017) and repository (T020) tests (plan "Risks"). Both call `Database.Migrate()`, which raises EF Core's pending-model-changes error if there is one; T022 re-checks this through `DatabaseMigrator`. If it reports pending changes, run (from `src/backend`) `dotnet ef migrations remove --project src/ShopManagement.Infrastructure --startup-project src/ShopManagement.Api`, then T019's add command. The T017 schema tests must stay green.
- [x] T022 Write tests for `DatabaseMigrator` — IntegrationTests/AdminAccounts/DatabaseMigratorTests.cs:
  - The migrator is built as in T023 (configuration + `AppDbContext` + lock timeout).
  - Connection string absent or empty → failure `ConnectionStringMissing`, code `StoredData.ConnectionStringMissing`; building the `AppDbContext` for it does not throw.
  - A path to a file that does not exist → file created with the `AdminAccounts` table (FR-016).
  - An existing empty file → migrated, the table exists (FR-016).
  - A row inserted first into `__EFMigrationsLock`, lock timeout 2 s → failure `StoredDataLocked`, code `StoredData.Locked`, within the timeout (FR-017).
  - ⚠ High risk (plan "Risks"): if EF Core 10.0.13's lock wait ignores the cancellation token, stop and report; the plan must be revised.
- [x] T023 Add `ConnectionStringMissing` and `StoredDataLocked` to `StoredDataErrors` and implement `DatabaseMigrator(IConfiguration, AppDbContext, TimeSpan lockTimeout)` (reads `ConnectionStrings:Default` from the configuration and checks it before touching the context; `Database.MigrateAsync` with a token that cancels after the lock timeout, default `DefaultLockTimeout` = 60 s, settable through the constructor; catches only the `OperationCanceledException` of its own timeout) so T022 passes — Infrastructure/Persistence/DatabaseMigrator.cs, Application/Common/StoredDataErrors.cs

Integration test support

- [x] T024 Change `HealthEndpointsTests` to create its own `SqliteTestDatabase` and a `ShopApiFactory` on it (both do not exist yet, so the test project fails to build) — IntegrationTests/HealthEndpointsTests.cs
- [x] T025 Implement the test support so T024 passes (research R10) — IntegrationTests/TestSupport/SqliteTestDatabase.cs, CapturingLoggerProvider.cs, TestSecrets.cs, ShopApiFactory.cs:
  - `SqliteTestDatabase`: one per test, created in the test (not an xUnit class fixture, which would share one database across the class); `IDisposable`; owns the sentinel connection to its own `Data Source=<guid>;Mode=Memory;Cache=Shared` database; exposes the connection string. A test that needs two databases (US1 #2) creates two; a restart test builds two factories on the same one.
  - `CapturingLoggerProvider`: keeps level, category and formatted message.
  - `TestSecrets`: sign-in name `op-` + 8 random letters and digits, password `Kx9!` + 12 random characters, generated at runtime.
  - `ShopApiFactory(SqliteTestDatabase database, string? signInName, string? initialPassword)`, with both values defaulting to new `TestSecrets` values: sets `ConnectionStrings:Default` to the database, replaces `SetUpAdminAccountCommand` with the given values through `ConfigureTestServices` (`null` = setting absent), adds the capturing provider and exposes its entries.

**Checkpoint:** `dotnet test src/backend` is green, including `ShopManagement.ArchitectureTests`; `dotnet build src/backend` has 0 warnings.

## Phase 3 — US1: Create the Admin account at first start (P1) 🎯 MVP

**Goal:** At a start with no Admin account and valid setup values, the system creates the single account (NFC, trimmed name, PBKDF2 hash) before it listens, and logs the creation without any value.
**Independent test:** Start the Api (process test) with an empty temporary database file and generated valid setup values: exactly one row exists with the trimmed name and a hash that recomputes from the password; the creation log precedes "Now listening on"; the output holds no value.

- [x] T026 [US1] Write validator tests for accepted values — each gives no problem: "admin" + 16-character password; "  Shop Admin  "; "admin\n"; " admin"; "owner"; 100-code-point name + 12-character password; 128-character password; "  my shop pass  "; NFD "café-shop-2026"; 11 single-code-point emoji + 1 letter (US1 #1, #3–#10) — Application.Tests/AdminAccounts/SetUpAdminAccount/SetUpAdminAccountValidatorTests.cs
- [x] T027 [US1] Implement the minimum of `SetUpAdminAccountValidator` that T026 needs (the `Validate` signature of contracts/application.md, no problem for valid values); collecting violations and their rule texts comes in T040 — Application/AdminAccounts/SetUpAdminAccount/SetUpAdminAccountValidator.cs
- [x] T028 [US1] Write handler tests for creation with NSubstitute fakes and a hand-written `FakeTimeProvider` (overrides `GetUtcNow()`) — Application.Tests/AdminAccounts/SetUpAdminAccount/SetUpAdminAccountHandlerTests.cs, Application.Tests/TestSupport/FakeTimeProvider.cs:
  - Call order with no account: `MigrateAsync` → `ListAsync` → validate → `Hash` → `AddAsync` (contracts/application.md).
  - `Hash` receives the NFC password with its spaces; the added account has the trimmed NFC name with its case.
  - `Added` → `Success(Created)` with `CreatedAtUtc` equal to the fake time (FR-013).
  - A `MigrateAsync` failure and a `ListAsync` failure are returned as is, and nothing after them is called (FR-014, FR-016).
  - `Hash` throws → `AddAsync` is not called (FR-012).
- [x] T029 [US1] Create `SetUpAdminAccountStatus` and `SetUpAdminAccountOutcome` (contracts/application.md) and implement `SetUpAdminAccountHandler` steps 1, 2 and the creation path of step 5 (its refusal branch comes in T041; steps 3–4 and the re-read after `AlreadyExists` in T047) so T028 passes — Application/AdminAccounts/SetUpAdminAccount/SetUpAdminAccountHandler.cs, SetUpAdminAccountStatus.cs, SetUpAdminAccountOutcome.cs
- [x] T030 [US1] Extend the DI test: after `AddApplication()`, the `ServiceCollection` holds `SetUpAdminAccountHandler` and `SetUpAdminAccountValidator` descriptors with `ServiceLifetime.Scoped`. Descriptors only: `BuildServiceProvider` is not available in Application.Tests (only the DI Abstractions package is referenced); resolving the handler is checked in T033 — Application.Tests/DependencyInjectionTests.cs
- [ ] T031 [US1] Register `SetUpAdminAccountValidator` and `SetUpAdminAccountHandler` (scoped) in `AddApplication()` so T030 passes — Application/DependencyInjection.cs
- [ ] T032 [P] [US1] Add the process-test helpers (research R8): `RepositoryRoot` (walks up to `ShopManagement.slnx`, returns the Api build output for the test assembly's configuration, fails with a clear message if the dll is missing) and `ApiProcess` (runs `dotnet ShopManagement.Api.dll` with the inherited environment minus every `AdminSetup__*`, `ConnectionStrings__*`, `ASPNETCORE_*`, `DOTNET_ENVIRONMENT`, `Logging__*`; sets `ASPNETCORE_ENVIRONMENT=Production`, `ASPNETCORE_URLS=http://127.0.0.1:0` and a temporary SQLite file; captures stdout, stderr and the exit code; waits up to 60 s for exit or for "Now listening on", then kills) — IntegrationTests/TestSupport/RepositoryRoot.cs, IntegrationTests/TestSupport/ApiProcess.cs
- [ ] T033 [US1] Write in-process setup tests, each on its own `SqliteTestDatabase` and a `ShopApiFactory` with the values it needs — IntegrationTests/AdminAccounts/AdminAccountSetupTests.cs:
  - `SetUpAdminAccountHandler` resolves from `factory.Services.CreateScope()` (DI wiring of T031 and T035).
  - US1 #1: one row with the given name; algorithm, 16-byte salt, 600,000 iterations, 32-byte hash; PBKDF2 recomputed with the stored salt matches the password.
  - US1 #2: two factories on two databases with the same password → different salts and different hashes.
  - US1 #3: "  Shop Admin  " → stored "Shop Admin".
  - US1 #6: 100-code-point name and 12-character password → created.
  - US1 #8: the hash matches "  my shop pass  " and not "my shop pass".
  - US1 #9: NFD "café-shop-2026" → created; the hash matches the NFC form.
  - US1 #10: 11 emoji + 1 letter → created.
  - US1 #11: the captured log has "Admin account created at <ISO 8601 time>" and the reminder to remove `AdminSetup:InitialPassword`; it holds neither the name, the password, the hash (base64 and hex), nor "Size =" (FR-013, NFR-003).
  - SC-001: no stored column equals the readable password.
- [ ] T034 [US1] Write process tests — IntegrationTests/AdminAccounts/AdminAccountSetupProcessTests.cs:
  - US1 #11: the creation message appears before "Now listening on"; stdout and stderr hold neither generated value nor the hash (NFR-003).
  - FR-016 / SC-004: a database file that does not exist, in an empty temporary folder, with only `ConnectionStrings__Default` and the two `AdminSetup__*` values set → the file is created, the account is created, the process listens.
- [ ] T035 [US1] Register in `AddInfrastructure()`: `AppDbContext` (scoped) on SQLite with `ConnectionStrings:Default` read when the context is resolved, using `UseSqlite(connectionString)` when the value is present and the parameterless `UseSqlite()` when it is absent or empty, so resolving the handler never throws and `DatabaseMigrator` reports `ConnectionStringMissing`; `IDatabaseMigrator` → `DatabaseMigrator` (scoped, `DefaultLockTimeout`); `IAdminAccountRepository` → `AdminAccountRepository` (scoped); `IPasswordHasher` → `Pbkdf2PasswordHasher` (singleton, stateless); `TimeProvider.System` (singleton) — Infrastructure/DependencyInjection.cs
- [ ] T036 [US1] Set the EF Core log levels of research R7 (`Microsoft.EntityFrameworkCore`: `Warning`; `Microsoft.EntityFrameworkCore.Database.Command`: `None`; `Microsoft.EntityFrameworkCore.Update`: `None`); no setup value and no connection string — Api/appsettings.json
- [ ] T037 [US1] Add the startup step to `Program.cs` so T033 and T034 pass: build the setup-values configuration from environment variables plus user secrets in Development only, and register `SetUpAdminAccountCommand` as a singleton (research R2); between `Build()` and `RunAsync()`, resolve the command and handler in a scope and run the handler; on `Created` log the Information message of contracts/startup.md; on a failure dispose the app and return 1 (messages come in T044 and T049); the top-level statements return 0 or 1. This is the first task that runs the startup step under `WebApplicationFactory`: it retires the plan risk "`WebApplicationFactory` and the startup step"; if the factory's connection string or command are not what the step sees, stop and report — Api/Program.cs

**Checkpoint:** `dotnet test src/backend` is green. Manually: with `ConnectionStrings:Default` and both `AdminSetup` values in user secrets, `dotnet run --project src/backend/src/ShopManagement.Api` creates the database file and logs the creation before "Now listening on".

## Phase 4 — US2: Refuse to start without valid setup values (P1)

**Goal:** With no Admin account, any missing or invalid setup value (or a missing connection string, or an unexpected start error) refuses the start: every problem is logged by setting and rule, no account is stored, and the process exits with code 1 before it listens.
**Independent test:** Run the process tests of T043: each refused start exits with a non-zero code, never logs "Now listening on", stores no row, and names the setting and rule without the value.

- [ ] T038 [US2] Extend the validator tests with every rule text of contracts/application.md — Application.Tests/AdminAccounts/SetUpAdminAccount/SetUpAdminAccountValidatorTests.cs:
  - No values → both settings "is missing" (US2 #1); name only → `AdminSetup:InitialPassword` "is missing" (US2 #2); `""` counts as missing for each setting.
  - 11 characters → "is shorter than 12 characters" (US2 #3); 129 → "is longer than 128 characters" (US2 #4).
  - Whitespace-only name → "is empty" (US2 #5); 101 code points after trimming → "is longer than 100 characters" (US2 #6).
  - Password + "\n" (US2 #9), a tab inside the password (US2 #10), name "shop\tadmin" (US2 #11) → "contains a control character".
  - An unpaired surrogate in either setting → "is not valid Unicode text" (plan interpretation 2).
  - Empty name + 5-character password → both settings' problems in one result (edge case "both invalid").
  - 12 spaces as password → no problem (edge case).
  - No rule text contains the value, its length or a position.
- [ ] T039 [P] [US2] Extend the handler tests: no account and invalid values → failure with code `AdminSetup.InvalidSetupValues` and `FieldErrors` equal to the validator's result; `Hash` and `AddAsync` are not called (FR-004); name only → the `AdminSetup:InitialPassword` "is missing" problem (US2 #2) — Application.Tests/AdminAccounts/SetUpAdminAccount/SetUpAdminAccountHandlerTests.cs
- [ ] T040 [US2] Implement the rule texts in `SetUpAdminAccountValidator` (missing and `InvalidText` set by the validator itself, data-model.md) so T038 passes — Application/AdminAccounts/SetUpAdminAccount/SetUpAdminAccountValidator.cs
- [ ] T041 [US2] Create `AdminSetupErrors` with `InvalidSetupValues(fieldErrors)` (code `AdminSetup.InvalidSetupValues`) and implement the refusal branch of handler step 5 (validation failure → that failure, nothing hashed or stored) so T039 passes — Application/AdminAccounts/SetUpAdminAccount/AdminSetupErrors.cs, SetUpAdminAccountHandler.cs
- [ ] T042 [P] [US2] Write a test that no committed `appsettings*.json` of the Api contains an `AdminSetup` or a `ConnectionStrings` section (FR-003, NFR-003, P7) — IntegrationTests/AdminAccounts/DeliveredConfigurationTests.cs
- [ ] T043 [US2] Write process tests for refused starts — IntegrationTests/AdminAccounts/AdminAccountSetupProcessTests.cs. Each one checks: exit code ≠ 0, "Now listening on" never logged, 0 rows in the database file (or no file created, when the connection string is missing), "The system did not start." logged, no generated value in stdout or stderr (NFR-003):
  - US2 #1: no setup values → both settings logged "is missing".
  - US2 #3: 11-character password → "is shorter than 12 characters"; the output contains neither "11 characters" nor "length".
  - US2 #7: `AdminSetup` values only in the `appsettings.json` of a temporary content root (`ASPNETCORE_CONTENTROOT`) → refused as missing.
  - FR-003: `AdminSetup` values passed only as command-line arguments → refused as missing (no setup command).
  - US2 #8: a refused start, then the same database file with valid values → created and listening.
  - US2 #9: a 16-character password followed by "\n" → "contains a control character", without value or position.
  - Edge "both invalid": empty name + 5-character password → both problems logged in the same start.
  - Missing connection string → "ConnectionStrings:Default is missing".
- [ ] T044 [US2] Add the refusal mapping to `Program.cs` so T043 passes (contracts/startup.md): one Critical line `Admin account setup refused: {Setting} {Rule}.` per problem in `FieldErrors`; the Critical messages for `ConnectionStringMissing` and `StoredDataLocked`; a single `catch (Exception)` around the startup step that logs `The stored data could not be prepared or read.` at Critical with the exception; then `The system did not start.`, `DisposeAsync` and return 1. The `StoredDataLocked` message has no process test (the 60 s lock timeout equals the `ApiProcess` wait); the migrator side is covered by T022 — Api/Program.cs

**Checkpoint:** `dotnet test src/backend` is green; every US2 process test passes.

## Phase 5 — US3: Leave the existing Admin account unchanged (P1)

**Goal:** Once the Admin account exists, the setup values never change it and are not checked; two simultaneous first starts leave exactly one account; damaged stored data refuses the start (FR-014).
**Independent test:** Create the account, restart (new factory on the same database) with different, missing and invalid setup values: the stored row is byte-for-byte unchanged and there is still one row.

- [ ] T045 [US3] Extend the handler tests — Application.Tests/AdminAccounts/SetUpAdminAccount/SetUpAdminAccountHandlerTests.cs:
  - One account with a usable password → `Success(AlreadyExists)`; validator, `Hash` and `AddAsync` are not called, with valid different values, no values and a 5-character password (US3 #1–#3, FR-009, FR-010).
  - Two accounts → failure with code `AdminSetup.MultipleAdminAccounts`; one account with an unusable password → failure with code `AdminSetup.UnusableStoredPassword` (FR-014).
  - `AddAsync` returns `AlreadyExists` → `ListAsync` is called exactly once more and the result is `Success(AlreadyExists)` with no `CreatedAtUtc` (research R4).
- [ ] T046 [US3] Write in-process restart tests — IntegrationTests/AdminAccounts/AdminAccountSetupTests.cs:
  - US3 #1: restart with a different name and a different valid password → name, salt and hash unchanged; the original password still matches.
  - US3 #2: restart with no setup values → starts, unchanged.
  - US3 #3: restart with a 5-character password → starts, unchanged.
  - US3 #4: 3 restarts → exactly 1 row after each.
  - Concurrent starts (FR-001), without `ShopApiFactory` (its own start would already create the row): build a `ServiceCollection` with `AddLogging` configured from the `Logging` section of the Api's `appsettings.json` plus `CapturingLoggerProvider`, `AddApplication()`, and `AddInfrastructure(config)` with `ConnectionStrings:Default` pointing at a temporary SQLite file; migrate once; then run the handler from two scopes in parallel → 1 row, one `Created` and one `AlreadyExists`; the captured log has no Error entry and no "Size =" (research R4, R7).
  - Primary-key race log, deterministic (R7): in the same harness, after the row exists, `AddAsync` of a second account → `AlreadyExists`; the captured log has no Error entry and no "Size =".
- [ ] T047 [US3] Add `MultipleAdminAccounts` and `UnusableStoredPassword` to `AdminSetupErrors` and implement handler steps 3–4 and the single re-read after `AlreadyExists`, in the order of contracts/application.md (the unusable-password check comes after the place of 019 hook A) so T045 and T046 pass — Application/AdminAccounts/SetUpAdminAccount/SetUpAdminAccountHandler.cs, AdminSetupErrors.cs
- [ ] T048 [US3] Write process tests for damaged stored data (FR-014) — IntegrationTests/AdminAccounts/AdminAccountSetupProcessTests.cs. Each one: exit code ≠ 0, never listening, "The system did not start." logged, no value in the output:
  - A database file of random bytes → "The stored data could not be prepared or read."
  - The table rebuilt without the CHECK, holding 2 rows → "The stored data holds more than one Admin account."
  - `PasswordIterations` set to 1,000 → "The stored Admin password cannot be used: …"
  - `SignInName` NULL (table rebuilt without NOT NULL) → "The stored Admin account cannot be read."
- [ ] T049 [US3] Add the Critical messages for `UnreadableStoredData`, `MultipleAdminAccounts` and `UnusableStoredPassword` (contracts/startup.md) to the refusal mapping so T048 passes — Api/Program.cs

**Checkpoint:** `dotnet test src/backend` is green.

## Phase 6 — US4: Warn about an initial password left in the setup values (P2)

**Goal:** At every start where the account exists and `AdminSetup:InitialPassword` is set and not empty, the system starts and logs a warning without the value.
**Independent test:** With an existing account, start once with an initial password set and once without: the warning appears only in the first start and never holds the value.

- [ ] T050 [US4] Extend the handler tests: account exists → `InitialPasswordStillPresent` is true for a non-empty initial password (equal to or different from the current one) and false for `null` and `""` (FR-011) — Application.Tests/AdminAccounts/SetUpAdminAccount/SetUpAdminAccountHandlerTests.cs
- [ ] T051 [US4] Write in-process warning tests — IntegrationTests/AdminAccounts/AdminAccountSetupTests.cs:
  - US4 #1, #2: initial password set, equal to or different from the stored one → the Warning of contracts/startup.md, without the value.
  - US3 #3: a 5-character initial password → the same warning, without the value.
  - US4 #3: only the sign-in name set → no warning.
  - US4 #4: two restarts with the password set → a warning at both.
  - Edge "empty value": `""` with an existing account → no warning.
- [ ] T052 [P] [US4] Write a process test: account exists, initial password still set → the process listens, the warning is logged, stdout and stderr hold neither generated value (NFR-003) — IntegrationTests/AdminAccounts/AdminAccountSetupProcessTests.cs
- [ ] T053 [US4] Set `InitialPasswordStillPresent = !string.IsNullOrEmpty(command.InitialPassword)` in handler step 4 so T050 passes — Application/AdminAccounts/SetUpAdminAccount/SetUpAdminAccountHandler.cs
- [ ] T054 [US4] Log the Warning message of contracts/startup.md when `InitialPasswordStillPresent` is true, so T051 and T052 pass — Api/Program.cs

**Checkpoint:** `dotnet test src/backend` is green.

## Phase 7 — Polish

- [ ] T055 [P] Write a guard test: no route in the `EndpointDataSource` of `ShopApiFactory` matches `register`, `signup`, `sign-up`, `users`, `roles`, or `POST` on an `accounts` collection (SC-003, FR-001, P1) — IntegrationTests/AdminAccounts/NoAccountCreationRoutesTests.cs
- [ ] T056 Replace "Run tests" in `CLAUDE.md` with the backend coverage command of research R9 (ReportGenerator, 80% line threshold, migrations and `DesignTimeDbContextFactory` excluded). Kit configuration: its own `chore:` commit; if a hook blocks it, ask the user — CLAUDE.md
- [ ] T057 Run the new "Run tests" command and the "Lint / typecheck" command: backend line coverage > 80% (P3), 0 warnings (P9); add tests for any uncovered production branch that is reachable in-process — src/backend/tests/
- [ ] T058 Review every log message and error text against contracts/startup.md and NFR-003: no value, length or hash; no `EnableSensitiveDataLogging`; no empty `catch`; no `FromSql*`, `SqlQuery*` or `ExecuteSql*` in `src/backend/src/` (P6, research R3); the values used for log and storage searches always come from `TestSecrets` (SC-001) — src/backend/

**Checkpoint:** run `/sdd-verify`.

## Dependencies

- Phase 1 → Phase 2 → US1 → US2 → US3 → US4 → Polish. US2, US3 and US4 each extend the handler, the handler tests and `Program.cs` that US1 creates, so they run in order.
- Inside Phase 2: T007 → T010 (`Create` uses the rules); T009 → T010; T011 + T014 → T016, T018, T020; T019 needs T001 (dotnet-ef) and T018; T020–T023 need T019; the T018 redo and T021 run back to back; T019 is not rerun after the redo unless T021's pending-changes check requires it; T022/T023 need T021 (T023 extends `StoredDataErrors`, and the model must be final); T025 needs T013 (`SetUpAdminAccountCommand`) and T019.
- T042 needs `RepositoryRoot` (T032) to find the committed `appsettings*.json` files.
- T017 must stay green after the T018 redo and T021.
- T022/T023 are high risk: if the migration lock ignores cancellation, stop the build and revise the plan.
- T034, T043, T048, T052 need T032 (process helpers) and a build of the Api (`dotnet test` builds it, as the test project references the Api).
- T037, T044, T049 and T054 all change `Program.cs`; T047 and T053 change the handler: never run them in parallel.
- T056 changes `CLAUDE.md` and goes in its own `chore:` commit (plan "Risks").

## Traceability

| Requirement / scenario | Tasks |
|------------------------|-------|
| FR-001 | T017, T018, T019, T020, T021, T046, T047, T055 |
| FR-002 | T029, T033, T034, T037 |
| FR-003 | T037, T042, T043 (US2 #7, command-line values) |
| FR-004 | T039, T041, T043, T044 |
| FR-005 | T038, T040, T043, T044 |
| FR-006 | T006, T007, T026, T038 |
| FR-007 | T006, T007, T026, T038 |
| FR-008 | T012, T013, T015, T016, T033 |
| FR-009 | T045, T046, T047 |
| FR-010 | T045, T046, T047 |
| FR-011 | T050, T051, T052, T053, T054 |
| FR-012 | T020, T021, T028, T029 |
| FR-013 | T028, T029, T033, T034, T037 |
| FR-014 | T008, T009, T020, T021, T028, T044, T045, T047, T048, T049 |
| FR-015 | T006, T007, T026, T033 |
| FR-016 | T022, T023, T028, T034 |
| FR-017 | T022, T023, T043, T044, T048 |
| NFR-001 | T008, T009, T015, T016, T033 |
| NFR-002 | T015, T016, T033 |
| NFR-003 | T025, T033, T034, T036, T042, T043, T046, T048, T051, T052, T058 |
| SC-001 | T025, T033, T058 |
| SC-002 | T043 |
| SC-003 | T055 |
| SC-004 | T001, T019, T023, T034 |
| US1 #1 | T026, T033 |
| US1 #2 | T015, T033 |
| US1 #3 | T006, T026, T033 |
| US1 #4 | T006, T026 |
| US1 #5 | T006, T026 |
| US1 #6 | T006, T026, T033 |
| US1 #7 | T006, T026 |
| US1 #8 | T006, T015, T026, T033 |
| US1 #9 | T006, T026, T033 |
| US1 #10 | T006, T026, T033 |
| US1 #11 | T033, T034 |
| US2 #1 | T038, T043 |
| US2 #2 | T038, T039 |
| US2 #3 | T038, T043 |
| US2 #4 | T038 |
| US2 #5 | T038 |
| US2 #6 | T038 |
| US2 #7 | T043 |
| US2 #8 | T043 |
| US2 #9 | T038, T043 |
| US2 #10 | T038 |
| US2 #11 | T038 |
| US3 #1 | T045, T046 |
| US3 #2 | T045, T046 |
| US3 #3 | T045, T046, T051 |
| US3 #4 | T046 |
| US4 #1 | T050, T051 |
| US4 #2 | T050, T051 |
| US4 #3 | T051 |
| US4 #4 | T051 |
| Edge: both invalid | T038, T043 |
| Edge: spaces-only password | T006, T038 |
| Edge: trailing line break in password | T038, T043 |
| Edge: empty value | T038, T050, T051 |
| Edge: interrupted creation | T020, T028 |
| Edge: concurrent starts | T020, T046 |
| Edge: no stored data / older structure | T022, T034 (older structure: first testable with 001's second migration, plan "Risks") |
| Edge: damaged stored data / several accounts | T020, T021, T045, T047, T048, T049 |
| Edge: reset indication with no account | — (019; ignored like any unknown setting, nothing to build) |
| Edge: sign-in name changed later | T045, T046 (US3 #1) |
| P3 coverage | T056, T057 |
| Setup support (no FR of its own) | T002 (user secrets for the development setup, FR-003), T003 (keeps SQLite files and test results out of git, P7), T024/T025 (test factory for every in-process test), T032 (process tests for FR-017) |

## Revision history

| Rev | Date | Plan rev | Change |
|-----|------|----------|--------|
| 1 | 2026-10-17 | 1 | Initial version |
| 2 | 2026-10-17 | 2 | Plan rev 2: T018 reset to redo (`AdminAccountRecord` becomes the write model with non-nullable properties). T021 adds the keyless read model `AdminAccountRow` (`ToView("AdminAccounts")`), `AppDbContext.AdminAccountRows`, and the pending-model-changes check, with regeneration of `AddAdminAccounts` if needed. Dependencies updated; T058 checks for raw SQL; the damaged-data traceability row is completed. Known deviations from the plan, kept from rev 1 and to fix at the next plan amend: T025 creates `SqliteTestDatabase` per test, not as a class fixture (plan R10, file structure); T030 checks DI descriptors only (plan file structure says "resolves the handler"; T033 resolves it); the plan's "Risks" names migrator tests for the read-model check, but those come in T022. |
