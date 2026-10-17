namespace ShopManagement.Domain.AdminAccounts;

public sealed class AdminAccount
{
    public const int SingleId = 1;

    private AdminAccount(string signInName, StoredPassword password)
    {
        SignInName = signInName;
        Password = password;
    }

    public int Id => SingleId;

    public string SignInName { get; }

    public StoredPassword Password { get; }

    /// <summary>Creates a new account. Throws <see cref="ArgumentException"/> when the values break the rules (a bug, not an expected failure).</summary>
    public static AdminAccount Create(string signInName, StoredPassword password)
    {
        ArgumentNullException.ThrowIfNull(signInName);
        ArgumentNullException.ThrowIfNull(password);

        if (AdminCredentialRules.NormalizeSignInName(signInName) != signInName
            || AdminCredentialRules.ValidateSignInName(signInName).Count > 0)
        {
            throw new ArgumentException("The sign-in name breaks the credential rules.", nameof(signInName));
        }

        if (!password.IsUsable)
        {
            throw new ArgumentException("The stored password is not usable.", nameof(password));
        }

        return new AdminAccount(signInName, password);
    }

    /// <summary>Rebuilds an account read from storage, without any rule check (FR-014).</summary>
    public static AdminAccount Restore(string signInName, StoredPassword password) =>
        new(signInName, password);
}
