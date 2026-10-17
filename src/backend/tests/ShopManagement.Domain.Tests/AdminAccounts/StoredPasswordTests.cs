using ShopManagement.Domain.AdminAccounts;

namespace ShopManagement.Domain.Tests.AdminAccounts;

public class StoredPasswordTests
{
    private static StoredPassword Build(
        string algorithm = "PBKDF2-HMAC-SHA256",
        int saltSize = 16,
        int iterations = 600_000,
        int hashSize = 32) =>
        new(algorithm, new byte[saltSize], iterations, new byte[hashSize]);

    [Fact]
    public void Constants_MatchTheStoredFormat()
    {
        Assert.Equal("PBKDF2-HMAC-SHA256", StoredPassword.Pbkdf2HmacSha256);
        Assert.Equal(16, StoredPassword.MinimumSaltSize);
        Assert.Equal(600_000, StoredPassword.MinimumIterations);
        Assert.Equal(32, StoredPassword.HashSize);
    }

    [Fact]
    public void Constructor_Values_AreExposed()
    {
        var salt = new byte[] { 1, 2, 3 };
        var hash = new byte[] { 4, 5 };

        var password = new StoredPassword("ALG", salt, 7, hash);

        Assert.Equal("ALG", password.Algorithm);
        Assert.Equal(salt, password.Salt);
        Assert.Equal(7, password.Iterations);
        Assert.Equal(hash, password.Hash);
    }

    [Fact]
    public void IsUsable_ExpectedFormat_IsTrue()
    {
        Assert.True(Build().IsUsable);
    }

    [Fact]
    public void IsUsable_LongerSaltAndMoreIterations_IsTrue()
    {
        Assert.True(Build(saltSize: 32, iterations: 1_000_000).IsUsable);
    }

    [Theory]
    [InlineData("PBKDF2-HMAC-SHA1")]
    [InlineData("SHA256")]
    [InlineData("")]
    public void IsUsable_OtherOrEmptyAlgorithm_IsFalse(string algorithm)
    {
        Assert.False(Build(algorithm: algorithm).IsUsable);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(0)]
    public void IsUsable_SaltTooShortOrEmpty_IsFalse(int saltSize)
    {
        Assert.False(Build(saltSize: saltSize).IsUsable);
    }

    [Fact]
    public void IsUsable_599999Iterations_IsFalse()
    {
        Assert.False(Build(iterations: 599_999).IsUsable);
    }

    [Theory]
    [InlineData(31)]
    [InlineData(33)]
    [InlineData(0)]
    public void IsUsable_HashNot32Bytes_IsFalse(int hashSize)
    {
        Assert.False(Build(hashSize: hashSize).IsUsable);
    }
}
