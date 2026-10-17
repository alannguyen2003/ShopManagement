using System.Security.Cryptography;
using System.Text;
using ShopManagement.Application.AdminAccounts;
using ShopManagement.Domain.AdminAccounts;

namespace ShopManagement.Infrastructure.Security;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    public StoredPassword Hash(string normalizedPassword)
    {
        var salt = RandomNumberGenerator.GetBytes(StoredPassword.MinimumSaltSize);
        var passwordBytes = Encoding.UTF8.GetBytes(normalizedPassword);
        try
        {
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                passwordBytes,
                salt,
                StoredPassword.MinimumIterations,
                HashAlgorithmName.SHA256,
                StoredPassword.HashSize);

            return new StoredPassword(
                StoredPassword.Pbkdf2HmacSha256,
                salt,
                StoredPassword.MinimumIterations,
                hash);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(passwordBytes);
        }
    }
}
