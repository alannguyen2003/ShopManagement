using System.Text;

namespace ShopManagement.Domain.AdminAccounts;

public static class AdminCredentialRules
{
    public const int SignInNameMaxLength = 100;
    public const int PasswordMinLength = 12;
    public const int PasswordMaxLength = 128;

    public static string? NormalizeSignInName(string value) => NormalizeText(value)?.Trim();

    public static string? NormalizePassword(string value) => NormalizeText(value);

    public static IReadOnlyList<CredentialRuleViolation> ValidateSignInName(string normalized)
    {
        var violations = new List<CredentialRuleViolation>();
        var length = normalized.EnumerateRunes().Count();

        if (length == 0)
        {
            violations.Add(CredentialRuleViolation.Empty);
        }
        else if (length > SignInNameMaxLength)
        {
            violations.Add(CredentialRuleViolation.TooLong);
        }

        if (HasControlCharacter(normalized))
        {
            violations.Add(CredentialRuleViolation.ControlCharacter);
        }

        return violations;
    }

    public static IReadOnlyList<CredentialRuleViolation> ValidatePassword(string normalized)
    {
        var violations = new List<CredentialRuleViolation>();
        var length = normalized.EnumerateRunes().Count();

        if (length < PasswordMinLength)
        {
            violations.Add(CredentialRuleViolation.TooShort);
        }
        else if (length > PasswordMaxLength)
        {
            violations.Add(CredentialRuleViolation.TooLong);
        }

        if (HasControlCharacter(normalized))
        {
            violations.Add(CredentialRuleViolation.ControlCharacter);
        }

        return violations;
    }

    private static string? NormalizeText(string value)
    {
        try
        {
            return value.Normalize(NormalizationForm.FormC);
        }
        catch (ArgumentException)
        {
            // An unpaired UTF-16 surrogate cannot be normalized (research R5).
            return null;
        }
    }

    private static bool HasControlCharacter(string value) =>
        value.EnumerateRunes().Any(Rune.IsControl);
}
