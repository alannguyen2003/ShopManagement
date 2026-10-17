namespace ShopManagement.Domain.AdminAccounts;

public enum CredentialRuleViolation
{
    Missing,
    Empty,
    TooShort,
    TooLong,
    ControlCharacter,
    InvalidText
}
