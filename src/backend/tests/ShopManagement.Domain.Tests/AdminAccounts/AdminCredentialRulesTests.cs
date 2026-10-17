using System.Text;
using ShopManagement.Domain.AdminAccounts;

namespace ShopManagement.Domain.Tests.AdminAccounts;

public class AdminCredentialRulesTests
{
    private const string Emoji = "\U0001F600";
    private const string LoneSurrogate = "ab\uD800cd";

    [Theory]
    [InlineData("  Shop Admin  ", "Shop Admin")]
    [InlineData("admin\n", "admin")]
    [InlineData(" admin", "admin")]
    [InlineData("\t admin\r\n", "admin")]
    [InlineData("Admin", "Admin")]
    public void NormalizeSignInName_Value_TrimsWhitespaceAndKeepsCase(string input, string expected)
    {
        Assert.Equal(expected, AdminCredentialRules.NormalizeSignInName(input));
    }

    [Fact]
    public void NormalizeSignInName_DecomposedText_ReturnsComposedForm()
    {
        var decomposed = "café".Normalize(NormalizationForm.FormD);

        var result = AdminCredentialRules.NormalizeSignInName(decomposed);

        Assert.Equal("café", result);
    }

    [Fact]
    public void NormalizePassword_DecomposedText_ReturnsComposedForm()
    {
        var decomposed = "café-shop-2026".Normalize(NormalizationForm.FormD);
        Assert.Equal(15, decomposed.EnumerateRunes().Count());

        var result = AdminCredentialRules.NormalizePassword(decomposed);

        Assert.NotNull(result);
        Assert.Equal(14, result.EnumerateRunes().Count());
        Assert.Equal("café-shop-2026", result);
    }

    [Fact]
    public void NormalizePassword_LeadingAndTrailingSpaces_AreKept()
    {
        Assert.Equal("  secret pass  ", AdminCredentialRules.NormalizePassword("  secret pass  "));
    }

    [Fact]
    public void NormalizeSignInName_UnpairedSurrogate_ReturnsNull()
    {
        Assert.Null(AdminCredentialRules.NormalizeSignInName(LoneSurrogate));
    }

    [Fact]
    public void NormalizePassword_UnpairedSurrogate_ReturnsNull()
    {
        Assert.Null(AdminCredentialRules.NormalizePassword(LoneSurrogate));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void ValidateSignInName_AllowedLength_HasNoViolation(int length)
    {
        Assert.Empty(AdminCredentialRules.ValidateSignInName(new string('a', length)));
    }

    [Fact]
    public void ValidateSignInName_NoCodePoints_ReportsEmpty()
    {
        var violations = AdminCredentialRules.ValidateSignInName("");

        Assert.Contains(CredentialRuleViolation.Empty, violations);
    }

    [Fact]
    public void ValidateSignInName_101CodePoints_ReportsTooLong()
    {
        var violations = AdminCredentialRules.ValidateSignInName(new string('a', 101));

        Assert.Contains(CredentialRuleViolation.TooLong, violations);
    }

    [Fact]
    public void ValidateSignInName_100Emoji_CountsCodePointsNotChars()
    {
        var name = string.Concat(Enumerable.Repeat(Emoji, 100));

        Assert.Empty(AdminCredentialRules.ValidateSignInName(name));
    }

    [Fact]
    public void ValidateSignInName_TabInside_ReportsControlCharacter()
    {
        var violations = AdminCredentialRules.ValidateSignInName("shop\tadmin");

        Assert.Contains(CredentialRuleViolation.ControlCharacter, violations);
    }

    [Fact]
    public void ValidateSignInName_PlainName_IsValidWithoutEmailCheck()
    {
        Assert.Empty(AdminCredentialRules.ValidateSignInName("owner"));
    }

    [Theory]
    [InlineData(12)]
    [InlineData(128)]
    public void ValidatePassword_AllowedLength_HasNoViolation(int length)
    {
        Assert.Empty(AdminCredentialRules.ValidatePassword(new string('a', length)));
    }

    [Fact]
    public void ValidatePassword_11CodePoints_ReportsTooShort()
    {
        var violations = AdminCredentialRules.ValidatePassword(new string('a', 11));

        Assert.Contains(CredentialRuleViolation.TooShort, violations);
    }

    [Fact]
    public void ValidatePassword_129CodePoints_ReportsTooLong()
    {
        var violations = AdminCredentialRules.ValidatePassword(new string('a', 129));

        Assert.Contains(CredentialRuleViolation.TooLong, violations);
    }

    [Theory]
    [InlineData("abcdefghijk\n")]
    [InlineData("abcdefghijk\r")]
    [InlineData("abcdefghijk\t")]
    public void ValidatePassword_LineBreakOrTab_ReportsControlCharacter(string password)
    {
        var violations = AdminCredentialRules.ValidatePassword(password);

        Assert.Contains(CredentialRuleViolation.ControlCharacter, violations);
    }

    [Fact]
    public void ValidatePassword_TwelveSpaces_IsValid()
    {
        Assert.Empty(AdminCredentialRules.ValidatePassword(new string(' ', 12)));
    }

    [Fact]
    public void ValidatePassword_ElevenEmojiAndALetter_IsValid()
    {
        var password = string.Concat(Enumerable.Repeat(Emoji, 11)) + "a";

        Assert.Empty(AdminCredentialRules.ValidatePassword(password));
    }

    [Fact]
    public void ValidatePassword_FormatCharacter_IsValid()
    {
        var password = "abcdefghijk​";

        Assert.Empty(AdminCredentialRules.ValidatePassword(password));
    }
}
