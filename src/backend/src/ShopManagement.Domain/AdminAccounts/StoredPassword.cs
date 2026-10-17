namespace ShopManagement.Domain.AdminAccounts;

public sealed class StoredPassword
{
    public const string Pbkdf2HmacSha256 = "PBKDF2-HMAC-SHA256";
    public const int MinimumSaltSize = 16;
    public const int MinimumIterations = 600_000;
    public const int HashSize = 32;

    public StoredPassword(string algorithm, byte[] salt, int iterations, byte[] hash)
    {
        Algorithm = algorithm;
        Salt = salt;
        Iterations = iterations;
        Hash = hash;
    }

    public string Algorithm { get; }

    public byte[] Salt { get; }

    public int Iterations { get; }

    public byte[] Hash { get; }

    public bool IsUsable =>
        Algorithm == Pbkdf2HmacSha256
        && Salt.Length >= MinimumSaltSize
        && Iterations >= MinimumIterations
        && Hash.Length == HashSize;
}
