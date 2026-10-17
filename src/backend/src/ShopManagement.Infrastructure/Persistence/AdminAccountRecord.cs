namespace ShopManagement.Infrastructure.Persistence;

// EF Core write model for the AdminAccounts table; its types match the IsRequired() columns.
public sealed class AdminAccountRecord
{
    public required int Id { get; set; }

    public required string SignInName { get; set; }

    public required string PasswordAlgorithm { get; set; }

    public required byte[] PasswordSalt { get; set; }

    public required int PasswordIterations { get; set; }

    public required byte[] PasswordHash { get; set; }
}
