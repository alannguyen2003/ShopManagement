namespace ShopManagement.Application.AdminAccounts.SetUpAdminAccount;

public sealed record SetUpAdminAccountOutcome(
    SetUpAdminAccountStatus Status,
    DateTimeOffset? CreatedAtUtc,
    bool InitialPasswordStillPresent);
