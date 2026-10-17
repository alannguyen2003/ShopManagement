using ShopManagement.Domain.AdminAccounts;
using ShopManagement.Domain.Common;

namespace ShopManagement.Application.AdminAccounts;

public interface IAdminAccountRepository
{
    // Failure UnreadableStoredData when a row has no sign-in name (research R3).
    Task<Result<IReadOnlyList<AdminAccount>>> ListAsync(CancellationToken cancellationToken);

    // AlreadyExists when another start inserted the row first (research R4).
    Task<AddAdminAccountOutcome> AddAsync(AdminAccount account, CancellationToken cancellationToken);
}
