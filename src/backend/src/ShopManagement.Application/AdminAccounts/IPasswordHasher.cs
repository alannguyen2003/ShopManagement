using ShopManagement.Domain.AdminAccounts;

namespace ShopManagement.Application.AdminAccounts;

public interface IPasswordHasher
{
    // normalizedPassword is already NFC (AdminCredentialRules.NormalizePassword).
    StoredPassword Hash(string normalizedPassword);
}
