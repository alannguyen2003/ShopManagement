namespace ShopManagement.Application.AdminAccounts.SetUpAdminAccount;

// A sealed class, not a record: ToString() must never print the values (research R2).
public sealed class SetUpAdminAccountCommand(string? signInName, string? initialPassword)
{
    public const string SignInNameSetting = "AdminSetup:SignInName";
    public const string InitialPasswordSetting = "AdminSetup:InitialPassword";

    public string? SignInName { get; } = signInName;

    public string? InitialPassword { get; } = initialPassword;

    public override string ToString() => nameof(SetUpAdminAccountCommand);
}
