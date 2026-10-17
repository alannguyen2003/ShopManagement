using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using ShopManagement.Api.IntegrationTests.TestSupport;
using ShopManagement.Application.AdminAccounts.SetUpAdminAccount;

namespace ShopManagement.Api.IntegrationTests.AdminAccounts;

public sealed class AdminAccountSetupTests
{
    private sealed record StoredRow(
        string SignInName, string Algorithm, byte[] Salt, int Iterations, byte[] Hash);

    private static ShopApiFactory StartHost(SqliteTestDatabase database, string? name, string? password)
    {
        var factory = new ShopApiFactory(database, name, password);
        using var client = factory.CreateClient();
        return factory;
    }

    private static List<StoredRow> ReadRows(SqliteTestDatabase database)
    {
        using var connection = new SqliteConnection(database.ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT SignInName, PasswordAlgorithm, PasswordSalt, PasswordIterations, PasswordHash FROM AdminAccounts";
        using var reader = command.ExecuteReader();
        var rows = new List<StoredRow>();
        while (reader.Read())
        {
            rows.Add(new StoredRow(
                reader.GetString(0),
                reader.GetString(1),
                (byte[])reader["PasswordSalt"],
                reader.GetInt32(3),
                (byte[])reader["PasswordHash"]));
        }

        return rows;
    }

    private static bool PasswordMatches(StoredRow row, string password) =>
        CryptographicOperations.FixedTimeEquals(
            Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                row.Salt,
                row.Iterations,
                HashAlgorithmName.SHA256,
                row.Hash.Length),
            row.Hash);

    private static string Emoji(int count) =>
        string.Concat(Enumerable.Repeat(char.ConvertFromUtf32(0x1F600), count));

    [Fact]
    public void Resolve_SetUpHandlerFromScope_ReturnsHandler()
    {
        using var database = new SqliteTestDatabase();
        using var factory = new ShopApiFactory(database);
        using var scope = factory.Services.CreateScope();

        var handler = scope.ServiceProvider.GetService<SetUpAdminAccountHandler>();

        Assert.NotNull(handler);
    }

    [Fact]
    public void Start_NoAccountAndValidValues_StoresOneAccountWithPbkdf2Parameters()
    {
        using var database = new SqliteTestDatabase();
        var name = TestSecrets.NewSignInName();
        var password = TestSecrets.NewPassword();
        using var factory = StartHost(database, name, password);

        var row = Assert.Single(ReadRows(database));

        Assert.Equal(name, row.SignInName);
        Assert.Equal("PBKDF2-HMAC-SHA256", row.Algorithm);
        Assert.Equal(16, row.Salt.Length);
        Assert.Equal(600_000, row.Iterations);
        Assert.Equal(32, row.Hash.Length);
        Assert.True(PasswordMatches(row, password));
    }

    [Fact]
    public void Start_SamePasswordOnTwoDatabases_StoresDifferentSaltsAndHashes()
    {
        var password = TestSecrets.NewPassword();
        using var databaseOne = new SqliteTestDatabase();
        using var databaseTwo = new SqliteTestDatabase();
        using var factoryOne = StartHost(databaseOne, TestSecrets.NewSignInName(), password);
        using var factoryTwo = StartHost(databaseTwo, TestSecrets.NewSignInName(), password);

        var rowOne = Assert.Single(ReadRows(databaseOne));
        var rowTwo = Assert.Single(ReadRows(databaseTwo));

        Assert.NotEqual(rowOne.Salt, rowTwo.Salt);
        Assert.NotEqual(rowOne.Hash, rowTwo.Hash);
    }

    [Fact]
    public void Start_NameWithSurroundingSpaces_StoresTrimmedName()
    {
        using var database = new SqliteTestDatabase();
        using var factory = StartHost(database, "  Shop Admin  ", TestSecrets.NewPassword());

        var row = Assert.Single(ReadRows(database));

        Assert.Equal("Shop Admin", row.SignInName);
    }

    [Fact]
    public void Start_NameOf100CodePointsAndPasswordOf12Characters_CreatesAccount()
    {
        using var database = new SqliteTestDatabase();
        var name = Emoji(50) + new string('a', 50);
        var password = TestSecrets.NewPassword()[..12];
        using var factory = StartHost(database, name, password);

        var row = Assert.Single(ReadRows(database));

        Assert.Equal(name, row.SignInName);
        Assert.True(PasswordMatches(row, password));
    }

    [Fact]
    public void Start_PasswordWithSurroundingSpaces_HashesTheSpacesToo()
    {
        using var database = new SqliteTestDatabase();
        using var factory = StartHost(database, TestSecrets.NewSignInName(), "  my shop pass  ");

        var row = Assert.Single(ReadRows(database));

        Assert.True(PasswordMatches(row, "  my shop pass  "));
        Assert.False(PasswordMatches(row, "my shop pass"));
    }

    [Fact]
    public void Start_PasswordInDecomposedForm_HashesTheComposedForm()
    {
        using var database = new SqliteTestDatabase();
        var decomposed = "cafe" + (char)0x0301 + "-shop-2026";
        var composed = "caf" + (char)0x00E9 + "-shop-2026";
        using var factory = StartHost(database, TestSecrets.NewSignInName(), decomposed);

        var row = Assert.Single(ReadRows(database));

        Assert.True(PasswordMatches(row, composed));
    }

    [Fact]
    public void Start_PasswordOf11EmojiAndOneLetter_CreatesAccount()
    {
        using var database = new SqliteTestDatabase();
        var password = Emoji(11) + "a";
        using var factory = StartHost(database, TestSecrets.NewSignInName(), password);

        var row = Assert.Single(ReadRows(database));

        Assert.True(PasswordMatches(row, password));
    }

    [Fact]
    public void Start_AccountCreated_LogsCreationTimeAndReminderWithoutSecrets()
    {
        using var database = new SqliteTestDatabase();
        var name = TestSecrets.NewSignInName();
        var password = TestSecrets.NewPassword();
        using var factory = StartHost(database, name, password);
        var row = Assert.Single(ReadRows(database));

        var messages = factory.Logs.Entries.Select(entry => entry.Message).ToList();

        Assert.Contains(messages, message => Regex.IsMatch(
            message,
            @"^Admin account created at \d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?(Z|[+-]\d{2}:\d{2})\. " +
            @"Remove AdminSetup:InitialPassword from the setup values now\.$"));
        var forbidden = new[]
        {
            name,
            password,
            Convert.ToBase64String(row.Hash),
            Convert.ToHexString(row.Hash),
            Convert.ToHexString(row.Hash).ToLowerInvariant(),
            "Size =",
        };
        foreach (var message in messages)
        {
            foreach (var value in forbidden)
            {
                Assert.DoesNotContain(value, message, StringComparison.Ordinal);
            }
        }
    }

    [Fact]
    public void Start_AccountCreated_NoStoredColumnEqualsThePassword()
    {
        using var database = new SqliteTestDatabase();
        var password = TestSecrets.NewPassword();
        using var factory = StartHost(database, TestSecrets.NewSignInName(), password);
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        var row = Assert.Single(ReadRows(database));

        Assert.NotEqual(password, row.SignInName);
        Assert.NotEqual(password, row.Algorithm);
        Assert.NotEqual<byte[]>(passwordBytes, row.Salt);
        Assert.NotEqual<byte[]>(passwordBytes, row.Hash);
        Assert.NotEqual(password, row.Iterations.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Assert.DoesNotContain(password, Convert.ToBase64String(row.Hash), StringComparison.Ordinal);
    }
}
