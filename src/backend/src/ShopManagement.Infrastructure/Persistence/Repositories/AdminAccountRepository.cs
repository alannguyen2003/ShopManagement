using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Application.AdminAccounts;
using ShopManagement.Application.Common;
using ShopManagement.Domain.AdminAccounts;
using ShopManagement.Domain.Common;

namespace ShopManagement.Infrastructure.Persistence.Repositories;

public sealed class AdminAccountRepository(AppDbContext context) : IAdminAccountRepository
{
    private const int PrimaryKeyViolation = 1555;

    public async Task<Result<IReadOnlyList<AdminAccount>>> ListAsync(CancellationToken cancellationToken)
    {
        var rows = await context.AdminAccountRows
            .AsNoTracking()
            .OrderBy(row => row.Id)
            .ToListAsync(cancellationToken);

        var accounts = new List<AdminAccount>(rows.Count);
        foreach (var row in rows)
        {
            if (row.SignInName is null)
            {
                return Result.Failure<IReadOnlyList<AdminAccount>>(StoredDataErrors.UnreadableStoredData);
            }

            var password = new StoredPassword(
                row.PasswordAlgorithm ?? string.Empty,
                row.PasswordSalt ?? [],
                row.PasswordIterations ?? 0,
                row.PasswordHash ?? []);
            accounts.Add(AdminAccount.Restore(row.SignInName, password));
        }

        return Result.Success<IReadOnlyList<AdminAccount>>(accounts);
    }

    public async Task<AddAdminAccountOutcome> AddAsync(AdminAccount account, CancellationToken cancellationToken)
    {
        var record = new AdminAccountRecord
        {
            Id = account.Id,
            SignInName = account.SignInName,
            PasswordAlgorithm = account.Password.Algorithm,
            PasswordSalt = account.Password.Salt,
            PasswordIterations = account.Password.Iterations,
            PasswordHash = account.Password.Hash
        };

        context.AdminAccounts.Add(record);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return AddAdminAccountOutcome.Added;
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is SqliteException { SqliteExtendedErrorCode: PrimaryKeyViolation })
        {
            context.Entry(record).State = EntityState.Detached;
            return AddAdminAccountOutcome.AlreadyExists;
        }
    }
}
