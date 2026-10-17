using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShopManagement.Application.Common;
using ShopManagement.Domain.Common;

namespace ShopManagement.Infrastructure.Persistence;

public sealed class DatabaseMigrator(IConfiguration configuration, AppDbContext context, TimeSpan lockTimeout)
    : IDatabaseMigrator
{
    public static readonly TimeSpan DefaultLockTimeout = TimeSpan.FromSeconds(60);

    public async Task<Result> MigrateAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(configuration.GetConnectionString("Default")))
        {
            return Result.Failure(StoredDataErrors.ConnectionStringMissing);
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(lockTimeout);

        try
        {
            await context.Database.MigrateAsync(timeout.Token);
            return Result.Success();
        }
        catch (OperationCanceledException) when (
            timeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            return Result.Failure(StoredDataErrors.StoredDataLocked);
        }
    }
}
