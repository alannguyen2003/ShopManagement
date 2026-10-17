namespace ShopManagement.Application.AdminAccounts.SetUpAdminAccount;

public sealed class SetUpAdminAccountValidator
{
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Validate(SetUpAdminAccountCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        // Rules and their texts are added in T040.
        return new Dictionary<string, IReadOnlyList<string>>();
    }
}
