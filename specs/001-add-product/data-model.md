# Data model: Add a product

**Feature:** 001-add-product
**Plan:** [plan.md](plan.md)

Code first: the entity below lives in `ShopManagement.Domain/Products/`. Its EF Core mapping lives in `Infrastructure/Persistence/Configurations/ProductConfiguration.cs`, and the migration `AddProducts` creates the table.

## Entity `Product` (table `Products`)

| Property | C# type | SQLite column | Null | Rule / source |
|----------|---------|---------------|------|---------------|
| `Id` | `Guid` | `Id` TEXT, PK | no | `Guid.CreateVersion7()` when created. Never shown in error messages (NFR-006). |
| `Name` | `string` | `Name` TEXT | no | Trimmed. 1–200 text elements (FR-004, FR-008). |
| `Sku` | `string` | `Sku` TEXT | no | Trimmed, letter case as typed. 1–50 text elements (FR-005, FR-008). |
| `NormalizedSku` | `string` | `NormalizedSku` TEXT, **unique index** `IX_Products_NormalizedSku` | no | `Sku` → NFC → `ToUpperInvariant()`. Set only by the entity (FR-005, BR-030, BR-045). |
| `Price` | `long` | `Price` INTEGER | no | Whole VND, 0–1,000,000,000 (FR-006). |
| `Description` | `string?` | `Description` TEXT | yes | Stored as entered (line breaks kept). An empty string becomes `null`. ≤ 2,000 text elements (FR-007). |
| `StockQuantity` | `int` | `StockQuantity` INTEGER | no | Always `1` on creation (FR-009, BR-001). No setter in this feature. |
| `Status` | `ProductStatus` | `Status` TEXT (enum stored as string) | no | Always `Draft` on creation (FR-010). |
| `CreatedAtUtc` | `DateTime` (UTC) | `CreatedAtUtc` TEXT | no | `TimeProvider.GetUtcNow().UtcDateTime` when added (FR-011). A value converter sets `DateTimeKind.Utc` on read, because SQLite returns `Unspecified`, so replay replies also serialize with "Z". 002 orders by it. |
| `CreateRequestId` | `Guid` | `CreateRequestId` TEXT, **unique index** `IX_Products_CreateRequestId` | no | The `Idempotency-Key` of the Save action that created it (FR-014). Internal only, never returned. |

Length limits are not declared as `HasMaxLength` constraints, because SQLite does not enforce them. They are enforced by the Application validator (text elements) and guarded by `Product.Create`.

### Enum `ProductStatus`

`Draft` only in this feature. 005-publish-product adds `Active`. Stored as its name, so adding values needs no data migration.

### Constants `ProductRules` (Domain)

`NameMaxLength = 200`, `SkuMaxLength = 50`, `DescriptionMaxLength = 2000`, `PriceMin = 0`, `PriceMax = 1_000_000_000`, `InitialStockQuantity = 1`. The validator and `Product.Create` both read them, so the rules exist in one place.

## Creation

`Product.Create(name, sku, price, description, createdAtUtc, createRequestId)`:
- trims `name` and `sku`, computes `NormalizedSku`, turns an empty `description` into `null`;
- sets `StockQuantity = 1`, `Status = Draft`, `Id = Guid.CreateVersion7()`;
- throws `ArgumentException` if a rule is broken. That is a bug: the validator has already rejected the input, so the domain only guards its invariants.

## State transitions

| From | To | Feature |
|------|----|---------|
| (none) | `Draft` | 001 (this feature) |
| `Draft` | `Active` | 005-publish-product (not here) |

## Relationships

None in this feature.

## Migration

`Infrastructure/Persistence/Migrations/<timestamp>_AddProducts.cs`, generated with `dotnet ef migrations add AddProducts --project src/ShopManagement.Infrastructure --startup-project src/ShopManagement.Api --output-dir Persistence/Migrations`. It creates the `Products` table with the two unique indexes. `DatabaseMigrator.MigrateDatabaseAsync()`, called from `Program.cs` before `RunAsync()`, applies pending migrations at startup with `MigrateAsync`.
