using System.Security.Cryptography;
using System.Text;
using ShopManagement.Infrastructure.Security;

namespace ShopManagement.Api.IntegrationTests.AdminAccounts;

public class Pbkdf2PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    private static string NewPassword() => "pw-" + Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

    private static byte[] Recompute(string password, byte[] salt, int iterations) =>
        Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA256, 32);

    [Fact]
    public void Hash_Password_UsesTheRequiredFormat()
    {
        var stored = _hasher.Hash(NewPassword());

        Assert.Equal("PBKDF2-HMAC-SHA256", stored.Algorithm);
        Assert.Equal(16, stored.Salt.Length);
        Assert.Equal(600_000, stored.Iterations);
        Assert.Equal(32, stored.Hash.Length);
        Assert.True(stored.IsUsable);
    }

    [Fact]
    public void Hash_Password_CanBeRecomputedFromUtf8SaltAndIterations()
    {
        var password = NewPassword();

        var stored = _hasher.Hash(password);

        Assert.Equal(stored.Hash, Recompute(password, stored.Salt, stored.Iterations));
    }

    [Fact]
    public void Hash_SamePasswordTenTimes_GivesDistinctSaltsAndHashes()
    {
        var password = NewPassword();

        var results = Enumerable.Range(0, 10).Select(_ => _hasher.Hash(password)).ToList();

        Assert.Equal(10, results.Select(r => Convert.ToBase64String(r.Salt)).Distinct().Count());
        Assert.Equal(10, results.Select(r => Convert.ToBase64String(r.Hash)).Distinct().Count());
    }

    [Fact]
    public void Hash_PasswordWithSurroundingSpaces_IsNotReproducedByTheTrimmedPassword()
    {
        var core = NewPassword();

        var stored = _hasher.Hash("  " + core + "  ");

        Assert.NotEqual(stored.Hash, Recompute(core, stored.Salt, stored.Iterations));
        Assert.Equal(stored.Hash, Recompute("  " + core + "  ", stored.Salt, stored.Iterations));
    }
}
