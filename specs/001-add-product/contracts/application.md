# Internal contracts: Add a product

Public signatures between layers and between frontend modules. Names follow `docs/architecture.md`.

## Domain (`ShopManagement.Domain`)

```csharp
// Domain/Common/Result.cs — change: `Error` gains an optional field-error map (stays sealed).
public sealed record Error(string Code, string Message,
                           IReadOnlyDictionary<string, string[]>? FieldErrors = null);

// Domain/Products/Product.cs
public sealed class Product
{
    public static Product Create(string name, string sku, long price, string? description,
                                 DateTime createdAtUtc, Guid createRequestId);
    public static string NormalizeSku(string sku); // Trim → NFC → ToUpperInvariant (research R2)
    // read-only properties: see data-model.md
}
```

## Application (`ShopManagement.Application/Products/`)

```csharp
public interface IProductRepository
{
    Task<Product?> FindByCreateRequestIdAsync(Guid createRequestId, CancellationToken cancellationToken);
    Task<bool> SkuExistsAsync(string normalizedSku, CancellationToken cancellationToken);
    /// Saves the product. Returns DuplicateSku / DuplicateCreateRequest when a UNIQUE index rejects it;
    /// any other failure is rethrown.
    Task<AddProductOutcome> AddAsync(Product product, CancellationToken cancellationToken);
}

public enum AddProductOutcome { Added, DuplicateSku, DuplicateCreateRequest }

public sealed record ProductDto(Guid Id, string Name, string Sku, long Price, string? Description,
                                int StockQuantity, string Status, DateTime CreatedAtUtc);

public static class ProductErrors
{
    public const string ValidationCode = "Products.Validation"; // → 400 ValidationProblem
    public const string SkuAlreadyUsedMessage = "SKU is already used by another product.";
}

// CreateProduct/
public sealed record CreateProductCommand(string? Name, string? Sku, long? Price, string? Description,
                                          Guid CreateRequestId);
public sealed record CreateProductResult(ProductDto Product, bool IsReplay);

public sealed class CreateProductValidator
{
    /// Format rules only (required, lengths, range). Empty map = valid. Also reports whether the SKU
    /// passed its own format rules, so the handler knows whether to check uniqueness.
    public IDictionary<string, List<string>> Validate(CreateProductCommand command);
}

public sealed class CreateProductHandler(IProductRepository products, CreateProductValidator validator,
                                         TimeProvider timeProvider)
{
    public Task<Result<CreateProductResult>> HandleAsync(CreateProductCommand command,
                                                         CancellationToken cancellationToken);
}
```

Handler flow (FR-012, FR-014):
1. `FindByCreateRequestIdAsync`. If a product is found, return it (`IsReplay = true`) without validating the body (research R3).
2. `validator.Validate`. If the SKU passed its format rules, `SkuExistsAsync`; a match adds `sku: SKU is already used…` to the same map.
3. If the map is not empty, return `Error(ValidationCode, "One or more fields are invalid.", map)` with every field at once.
4. `Product.Create(…, timeProvider.GetUtcNow().UtcDateTime, key)`, then `AddAsync`.
   - `DuplicateCreateRequest` → re-read the product and return the replay.
   - `DuplicateSku` → the same validation error with only `sku`.

## Api (`ShopManagement.Api`)

```csharp
// Endpoints/Products/CreateProductRequestReader.cs
public static class CreateProductRequestReader
{
    /// Reads the body itself (JsonDocument.ParseAsync, MaxDepth 8, AllowDuplicateProperties = false),
    /// so malformed JSON never reaches framework binding. Structural checks only: Content-Type JSON
    /// (else 415), object, allowed names, JSON types, Idempotency-Key UUID. Catches only JsonException.
    public static Task<Result<CreateProductCommand>> ReadAsync(HttpRequest request, CancellationToken ct);
}

// Contracts/Products/ProductResponse.cs
public sealed record ProductResponse(Guid Id, string Name, string Sku, long Price, string? Description,
                                     int StockQuantity, string Status, DateTime CreatedAt);

// Endpoints/ProblemMapping.cs
public static class ProblemMapping
{
    public static IResult ToProblem(this Error error); // FieldErrors → 400 ValidationProblem, fixed title
}

// Endpoints/ProductsEndpoints.cs
public static class ProductsEndpoints
{
    public static IEndpointRouteBuilder MapProductsEndpoints(this IEndpointRouteBuilder app);
    // POST /api/products (HttpRequest parameter) → 201 | 200 (replay) | 400 | 401 | 415
}
```

## Infrastructure (`ShopManagement.Infrastructure`)

```csharp
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration);
// registers AppDbContext (UseSqlite(ConnectionStrings:Default); throws InvalidOperationException naming the
// setting if missing), IProductRepository, TimeProvider.System, and DatabaseMigrationHostedService.

// Persistence/DatabaseMigrationHostedService.cs (internal) — IHostedService whose StartAsync runs
// Database.MigrateAsync() before Kestrel starts listening. Api calls nothing but AddInfrastructure().
```

`ProductRepository.AddAsync` catches only a `DbUpdateException` whose inner `SqliteException` has `SqliteExtendedErrorCode == 2067` (SQLITE_CONSTRAINT_UNIQUE). It detaches the entity, then checks whether the key is stored (→ `DuplicateCreateRequest`) or the normalized SKU is stored (→ `DuplicateSku`). If neither, it rethrows. `SqliteException` comes from `Microsoft.Data.Sqlite`, which arrives through the allowed `Microsoft.EntityFrameworkCore.Sqlite`, so no PackageReference is added.

## Frontend (`src/frontend/src/`)

```ts
// features/products/types.ts
export interface Product { id: string; name: string; sku: string; price: number;
  description: string | null; stockQuantity: number; status: 'Draft'; createdAt: string }
export interface CreateProductInput { name: string; sku: string; price: number | null; description: string }

// features/products/api.ts — calls apiRequest only
export function createProduct(input: CreateProductInput, idempotencyKey: string): Promise<Product>

// features/products/hooks.ts
export type FieldName = 'name' | 'sku' | 'price' | 'description'
export type SaveFailure =
  | { kind: 'validation'; fieldErrors: Partial<Record<FieldName, string[]>> } // 400 with a known field key
  | { kind: 'session-ended' } // 401: "Your session has ended. The product was not saved."; 017 opens the prompt
  | { kind: 'not-saved' }     // everything else: network error, 5xx, 403, 404, 415, 400 without a known field
export function useCreateProduct(): {
  save(input: CreateProductInput): Promise<Product | null>; // ignores calls while one is in flight
  isSaving: boolean; failure: SaveFailure | null }
// The idempotency key is created once per hook mount (crypto.randomUUID()) and reused for retries.

// shared/lib/vnd.ts
export function formatVnd(amount: number): string          // 125000 → "125.000 ₫"
export function groupDigits(digits: string): string         // "125000" → "125.000"
export function parsePastedPrice(text: string): string | null // "125.000" → "125000"; "125,5" / >15 digits → null
```
