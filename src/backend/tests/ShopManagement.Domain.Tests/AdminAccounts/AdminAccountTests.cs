using ShopManagement.Domain.AdminAccounts;

namespace ShopManagement.Domain.Tests.AdminAccounts;

public class AdminAccountTests
{
    private static StoredPassword UsablePassword() =>
        new(StoredPassword.Pbkdf2HmacSha256, new byte[16], 600_000, new byte[32]);

    private static StoredPassword UnusablePassword() =>
        new(StoredPassword.Pbkdf2HmacSha256, new byte[15], 600_000, new byte[32]);

    [Fact]
    public void SingleId_IsOne()
    {
        Assert.Equal(1, AdminAccount.SingleId);
    }

    [Fact]
    public void Create_ValidValues_UsesSingleIdAndKeepsNameAndPassword()
    {
        var password = UsablePassword();

        var account = AdminAccount.Create("Shop Admin", password);

        Assert.Equal(AdminAccount.SingleId, account.Id);
        Assert.Equal(1, account.Id);
        Assert.Equal("Shop Admin", account.SignInName);
        Assert.Same(password, account.Password);
    }

    [Theory]
    [InlineData("")]
    [InlineData("shop\tadmin")]
    public void Create_NameBreakingRules_ThrowsArgumentException(string name)
    {
        Assert.Throws<ArgumentException>(() => AdminAccount.Create(name, UsablePassword()));
    }

    [Fact]
    public void Create_NameLongerThan100CodePoints_ThrowsArgumentException()
    {
        var name = new string('a', 101);

        Assert.Throws<ArgumentException>(() => AdminAccount.Create(name, UsablePassword()));
    }

    [Fact]
    public void Create_UnusablePassword_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => AdminAccount.Create("admin", UnusablePassword()));
    }

    [Fact]
    public void Restore_ValidValues_UsesSingleIdAndKeepsNameAndPassword()
    {
        var password = UsablePassword();

        var account = AdminAccount.Restore("admin", password);

        Assert.Equal(AdminAccount.SingleId, account.Id);
        Assert.Equal("admin", account.SignInName);
        Assert.Same(password, account.Password);
    }

    [Fact]
    public void Restore_NameBreakingRules_DoesNotCheck()
    {
        var account = AdminAccount.Restore("shop\tadmin", UsablePassword());

        Assert.Equal("shop\tadmin", account.SignInName);
    }

    [Fact]
    public void Restore_UnusablePassword_DoesNotCheck()
    {
        var password = UnusablePassword();

        var account = AdminAccount.Restore("admin", password);

        Assert.False(account.Password.IsUsable);
        Assert.Same(password, account.Password);
    }
}
