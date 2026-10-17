using ShopManagement.Domain.Common;

namespace ShopManagement.Application.Common;

public interface IDatabaseMigrator
{
    // Creates the stored data if it does not exist and applies pending migrations (FR-016).
    // Failure: ConnectionStringMissing, StoredDataLocked (research R6).
    Task<Result> MigrateAsync(CancellationToken cancellationToken);
}
