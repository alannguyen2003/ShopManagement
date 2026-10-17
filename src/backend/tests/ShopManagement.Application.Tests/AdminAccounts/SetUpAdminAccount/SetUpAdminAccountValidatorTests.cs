using ShopManagement.Application.AdminAccounts.SetUpAdminAccount;

namespace ShopManagement.Application.Tests.AdminAccounts.SetUpAdminAccount;

public class SetUpAdminAccountValidatorTests
{
    private static readonly string NoBreakSpace = ((char)0x00A0).ToString();
    private static readonly string CombiningAcute = ((char)0x0301).ToString();

    private static readonly SetUpAdminAccountValidator Validator = new();

    private static string Sixteen => new('p', 16);

    private static void AssertAccepted(string? signInName, string? password)
    {
        var problems = Validator.Validate(new SetUpAdminAccountCommand(signInName, password));

        Assert.Empty(problems);
    }

    [Fact]
    public void Validate_AdminAndSixteenCharacterPassword_ReturnsNoProblem()
    {
        AssertAccepted("admin", Sixteen);
    }

    [Fact]
    public void Validate_NameWithSurroundingSpaces_ReturnsNoProblem()
    {
        AssertAccepted("  Shop Admin  ", Sixteen);
    }

    [Fact]
    public void Validate_NameWithTrailingLineBreak_ReturnsNoProblem()
    {
        AssertAccepted("admin\n", Sixteen);
    }

    [Fact]
    public void Validate_NameWithLeadingNoBreakSpace_ReturnsNoProblem()
    {
        AssertAccepted(NoBreakSpace + "admin", Sixteen);
    }

    [Fact]
    public void Validate_NameThatIsNotAnEmailAddress_ReturnsNoProblem()
    {
        AssertAccepted("owner", Sixteen);
    }

    [Fact]
    public void Validate_HundredCodePointNameAndTwelveCharacterPassword_ReturnsNoProblem()
    {
        AssertAccepted(new string('n', 100), new string('p', 12));
    }

    [Fact]
    public void Validate_HundredCodePointNameMadeOfEmoji_ReturnsNoProblem()
    {
        var emoji = char.ConvertFromUtf32(0x1F600);

        AssertAccepted(string.Concat(Enumerable.Repeat(emoji, 100)), Sixteen);
    }

    [Fact]
    public void Validate_PasswordOfMaximumLength_ReturnsNoProblem()
    {
        AssertAccepted("admin", new string('p', 128));
    }

    [Fact]
    public void Validate_PasswordWithSurroundingSpaces_ReturnsNoProblem()
    {
        AssertAccepted("admin", "  my shop pass  ");
    }

    [Fact]
    public void Validate_DecomposedPasswordOfFifteenCodePoints_ReturnsNoProblem()
    {
        var password = "cafe" + CombiningAcute + "-shop-2026";
        Assert.Equal(15, password.EnumerateRunes().Count());

        AssertAccepted("admin", password);
    }

    [Fact]
    public void Validate_ElevenEmojiAndOneLetter_ReturnsNoProblem()
    {
        var emoji = char.ConvertFromUtf32(0x1F600);
        var password = string.Concat(Enumerable.Repeat(emoji, 11)) + "a";
        Assert.Equal(12, password.EnumerateRunes().Count());

        AssertAccepted("admin", password);
    }
}
