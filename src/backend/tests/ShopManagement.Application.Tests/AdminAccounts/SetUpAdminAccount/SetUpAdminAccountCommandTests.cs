using ShopManagement.Application.AdminAccounts.SetUpAdminAccount;

namespace ShopManagement.Application.Tests.AdminAccounts.SetUpAdminAccount;

public class SetUpAdminAccountCommandTests
{
    [Fact]
    public void ToString_WithValues_ContainsNeitherSignInNameNorPassword()
    {
        var signInName = "name-" + Guid.NewGuid().ToString("N");
        var password = "pw-" + Guid.NewGuid().ToString("N");
        var command = new SetUpAdminAccountCommand(signInName, password);

        var text = command.ToString();

        Assert.DoesNotContain(signInName, text);
        Assert.DoesNotContain(password, text);
    }

    [Fact]
    public void ToString_WithValues_ReturnsOnlyTheTypeName()
    {
        var command = new SetUpAdminAccountCommand(Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"));

        Assert.Equal(nameof(SetUpAdminAccountCommand), command.ToString());
    }

    [Fact]
    public void Constructor_Values_AreExposed()
    {
        var signInName = Guid.NewGuid().ToString("N");
        var password = Guid.NewGuid().ToString("N");

        var command = new SetUpAdminAccountCommand(signInName, password);

        Assert.Equal(signInName, command.SignInName);
        Assert.Equal(password, command.InitialPassword);
    }

    [Fact]
    public void Constructor_NullValues_AreKeptAsNull()
    {
        var command = new SetUpAdminAccountCommand(null, null);

        Assert.Null(command.SignInName);
        Assert.Null(command.InitialPassword);
    }

    [Fact]
    public void SettingNames_AreTheConfigurationKeys()
    {
        Assert.Equal("AdminSetup:SignInName", SetUpAdminAccountCommand.SignInNameSetting);
        Assert.Equal("AdminSetup:InitialPassword", SetUpAdminAccountCommand.InitialPasswordSetting);
    }
}
