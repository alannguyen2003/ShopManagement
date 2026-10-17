using System.Security.Cryptography;

namespace ShopManagement.Api.IntegrationTests.TestSupport;

// Setup values generated at runtime, so no literal credential is in the source.
public static class TestSecrets
{
    private const string Alphanumeric = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const string PasswordCharacters = Alphanumeric + "!@#$%^&*";

    public static string NewSignInName() => "op-" + Random(Alphanumeric, 8);

    public static string NewPassword() => "Kx9!" + Random(PasswordCharacters, 12);

    private static string Random(string characters, int length) =>
        new(RandomNumberGenerator.GetItems<char>(characters, length));
}
