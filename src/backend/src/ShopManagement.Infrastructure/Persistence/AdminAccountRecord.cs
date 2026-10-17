namespace ShopManagement.Infrastructure.Persistence;

// EF Core entity for the AdminAccounts table. Nullable properties keep damaged stored data readable (research R3).
public sealed class AdminAccountRecord
{
    public int Id { get; set; }

    public string? SignInName { get; set; }

    public string? PasswordAlgorithm { get; set; }

    public byte[]? PasswordSalt { get; set; }

    public int? PasswordIterations { get; set; }

    public byte[]? PasswordHash { get; set; }
}
