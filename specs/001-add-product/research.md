# Research: Add a product

**Feature:** 001-add-product
**Plan:** [plan.md](plan.md)
**Created:** 2026-10-17

The user's direction: backend built **code first** (EF Core model → migrations) and data stored in **SQLite**. Both match `docs/architecture.md` v1.1.0. The decisions below cover what the direction and the architecture leave open.

## R1. Creating the schema: EF Core migrations vs `EnsureCreated`

| Option | Pros | Cons |
|--------|------|------|
| A. EF Core migrations, generated with the local `dotnet-ef` tool, applied at startup with `Database.MigrateAsync()` (through an Infrastructure `MigrateDatabaseAsync()` extension called from `Program.cs`) | Code first, as the user asked. The architecture already names `Infrastructure/Persistence/Migrations/`. Later features (002–012, 013+) change the schema without losing data. Integration tests run the real migrations. | Needs the `dotnet-ef` local tool (allowed since architecture v1.1.0). |
| B. `Database.EnsureCreated()` | No tool. | Cannot change an existing database: the first schema change in 002+ would need the database dropped. Not "migrations" as the architecture says. |

**Choice: A.** `dotnet-ef` goes in a local tool manifest (`.config/dotnet-tools.json`), never global. Architecture v1.1.0 allows it as a `tool`, in that manifest. Migrating at startup is enough for a single-instance, single-admin app with one SQLite file.

## R2. SKU uniqueness ignoring case and leading/trailing spaces (FR-005, FR-008, edge case "same moment from two tabs")

| Option | Pros | Cons |
|--------|------|------|
| A. Stored `NormalizedSku` column (trim → Unicode NFC → `ToUpperInvariant()`) with a **unique index** | The database enforces it, including two concurrent adds. Fast lookup with 10,000 products (NFR-002). Works for Vietnamese letters. | One extra column. |
| B. SQLite `COLLATE NOCASE` on `Sku` | No extra column. | NOCASE folds ASCII only: "ế" vs "Ế" would not collide. |
| C. Check in code only | Simple. | Two concurrent adds can both pass the check. |

**Choice: A.** The handler checks first, so the duplicate appears together with the other field errors (FR-012), and the unique index catches the race. These are deliberate interpretations, and 004-update-product must reuse `Product.NormalizeSku` unchanged:
- `string.Trim()` removes all Unicode white space (tabs, NBSP), not only U+0020.
- NFC makes composed and decomposed forms of the same Vietnamese letter equal.
- `ToUpperInvariant` is simple case mapping ("ß" and "ẞ" do not collide). On a unique-constraint failure the repository looks up which key collided, so it does not parse the error text (see R3). The `Sku` column keeps the text as typed after trimming (spec Assumptions). Deleted products (007) stay in the table, so their SKUs stay reserved without extra work (BR-045).

## R3. One Save action → at most one product, including a retry after a lost response (FR-014)

| Option | Pros | Cons |
|--------|------|------|
| A. Client-generated **`Idempotency-Key` header** (UUID) per Save action, stored on the product with a unique index. A repeated key returns the stored product. | Covers double click, a retry after a network error and a lost response. No extra table. The header keeps the JSON body limited to product information (FR-009). | The key must be kept on the product for good (16 bytes). |
| B. Disable the Save button only | Simple. | Does nothing for a lost response. FR-014 says the system must recognize the retry. |
| C. Separate idempotency table with expiry | Can expire old keys. | Extra table and clean-up for no requirement. |

**Choice: A, plus the disabled button for UX.**
- The add form creates one key when it is mounted. It keeps the key across failed saves and creates a new one when the form opens again ("add another", Back from the confirmation).
- The key is **required** on every add request, and a missing or non-UUID key is rejected (400). Requests that bypass the form follow the same rule (FR-018).
- The key is checked **before** field validation. If it is already stored, the system returns that stored product (200) whatever the body says, and the confirmation shows the values actually saved (FR-013). If the Admin changed a value after a failure whose save had in fact succeeded, the confirmation shows the stored values. This is the "same Save action" of FR-014.
- If two requests with the same key race, the unique index fails one of them, and the repository then returns the stored product. If the twin commits between the replay check and the SKU uniqueness check, the handler looks up the key again before reporting "SKU already used", so it returns the replay instead.

## R4. Rejecting unexpected fields and wrongly typed values with field messages (FR-009, US1 #6, US2 #10, NFR-006)

| Option | Pros | Cons |
|--------|------|------|
| A. Endpoint takes `HttpRequest`, and `CreateProductRequestReader` (Api) parses the body with `JsonDocument.ParseAsync`, then checks property names and JSON types | Names the unexpected field. Gives a price-field message for `1500.5`, `"abc"`, `1e30`. Controls the error text, so no type names leak. Duplicate property names can be rejected. | A small hand-written reader. |
| B. Typed record + `JsonUnmappedMemberHandling.Disallow` | Less code. | Binding fails before our code runs: a generic 400 with no field message, and the exception text names .NET types (P7). |

**Choice: A.** The reader parses the body itself, because Minimal API binding (even to `JsonElement`) parses JSON before our code runs. In Development (`ThrowOnBadRequest`), malformed JSON would then reach `UseExceptionHandler` as a 500. The reader only checks structure (allowed names `name`, `sku`, `price`, `description`; strings or null; price a JSON integer). Rule checks (lengths, range, required) live in the Application validator, so a valid-typed request gets all field messages at once (FR-012). A structurally wrong request (sent outside the form) is answered with its structural errors only. The form always sends structurally valid requests, so FR-012 (every invalid field at once, in the form) holds in full. Direct requests are still rejected with field-level reasons (FR-018, US2 #8, #10).

## R5. Where validation runs: client and server, or server only

| Option | Pros | Cons |
|--------|------|------|
| A. Server only, plus the price-input mask that FR-006a requires in the browser | One copy of the rules. The architecture says no business logic in the frontend. The server returns every field error at once (FR-012), well within NFR-002. | A round trip before messages appear. |
| B. Duplicate the rules in the form | Instant messages. | Two copies of the rules to keep in sync. |

**Choice: A.** The form shows the `errors` from the ProblemDetails reply next to each field. The only client-side checks are the ones FR-006a defines on the input itself: digits only, dot grouping, the paste rule.

## R6. Counting characters (spec Assumptions: "a Vietnamese letter with diacritics counts as 1")

`string.Length` counts UTF-16 units, so a decomposed "ế" (NFD) would count as 3. The validator counts **text elements** (`StringInfo.LengthInTextElements`), which matches "characters as the Admin sees them" in both NFC and NFD. The stored text is not changed. Only `NormalizedSku` uses NFC.

## R7. Date added (FR-011)

EF Core's SQLite provider cannot order by or compare `DateTimeOffset`, and 002 orders by date added. The column is a **UTC `DateTime`** (`CreatedAtUtc`), and its value comes from the BCL `TimeProvider`, registered in Infrastructure as `TimeProvider.System` and mocked with NSubstitute in tests. No new `IClock` interface is needed, because `TimeProvider` already is that abstraction (P8).

## R8. Connection string (P7)

The constitution lists connection strings among secrets, so `ConnectionStrings:Default` is **not** put in `appsettings*.json`. It comes from user-secrets in Development (the Web SDK supports `UserSecretsId` without a package) or from the environment variable `ConnectionStrings__Default`. If it is missing, startup fails with a message naming the setting. Integration tests supply an in-memory SQLite connection. `dotnet-ef` uses a design-time factory with `Data Source=:memory:` (generating a migration does not connect).

## R9. Authentication (FR-017, NFR-003, P5)

The user's decision: authentication belongs to **014-admin-sign-in**, which is built first. 001 adds no authentication scheme or sign-in setup of its own. It maps the product route group with `.RequireAuthorization()`, so it stays protected even if 014's fallback policy changes. 014 also owns the product management page and its route (014 FR-008, FR-018), the protected layout and the integration-test sign-in helpers; 001 extends them. The sign-in prompt over the open form is **017-session-expiry** FR-008. 001's part is to keep the form mounted, with its values, when a save is refused with 401 (FR-015).

## R10. Measuring coverage (P3)

The project has no coverage threshold yet. `coverlet.collector` writes Cobertura files per backend test project, but nothing merges them or fails below 80%, and Vitest has no coverage provider installed. This is project-wide, not specific to this feature, but this is the first feature to be built, so it needs a decision before `/sdd-verify`:
- Frontend: `@vitest/coverage-v8` (npm dev, same version as `vitest`), run by a separate `test:coverage` script (`vitest run --coverage`). `coverage.include` lists the production sources, so files no test loads still count, and `thresholds.lines` is 80.01, because P3 requires *more than* 80%. Coverage is not enabled in the config, so single-file TDD runs are not failed by the global threshold.
- Backend: coverlet writes one Cobertura file per test project into a results folder that is cleared before each run. `dotnet-reportgenerator-globaltool` (local tool) merges them, filtered to the four production assemblies minus generated migrations, and fails at or below 80% (`minimumCoverageThresholds:lineCoverage=80.01`; confirm the argument with `-help` when it is added). Exact commands: plan, "Coverage commands".

Both are allowed since architecture v1.1.0 (`@vitest/coverage-v8` as npm dev, `dotnet-reportgenerator-globaltool` as a local tool).
