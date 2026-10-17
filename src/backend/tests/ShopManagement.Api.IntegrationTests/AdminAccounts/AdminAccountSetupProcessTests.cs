using Microsoft.Data.Sqlite;
using ShopManagement.Api.IntegrationTests.TestSupport;

namespace ShopManagement.Api.IntegrationTests.AdminAccounts;

// Runs the built Api as a child process and checks the start-up output (US1 #11, FR-016, SC-004, NFR-003).
public sealed class AdminAccountSetupProcessTests
{
    private const string CreatedMessage = "Admin account created at ";
    private const string ListeningMarker = "Now listening on";

    private static (byte[] Hash, byte[] Salt, long Count) ReadStoredAccount(string databaseFile)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databaseFile,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false,
        }.ToString();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM AdminAccounts";
        var count = (long)countCommand.ExecuteScalar()!;

        using var rowCommand = connection.CreateCommand();
        rowCommand.CommandText = "SELECT PasswordHash, PasswordSalt FROM AdminAccounts LIMIT 1";
        using var reader = rowCommand.ExecuteReader();
        return reader.Read()
            ? ((byte[])reader["PasswordHash"], (byte[])reader["PasswordSalt"], count)
            : ([], [], count);
    }

    private static Dictionary<string, string?> SetupValues(string name, string password) => new()
    {
        ["AdminSetup__SignInName"] = name,
        ["AdminSetup__InitialPassword"] = password,
    };

    [Fact]
    public async Task Start_NoAccountAndValidValues_LogsCreationBeforeListeningAndLeaksNothing()
    {
        var name = TestSecrets.NewSignInName();
        var password = TestSecrets.NewPassword();
        using var api = ApiProcess.Start(SetupValues(name, password));

        await api.WaitForExitOrListeningAsync();

        Assert.True(api.Listened, $"The Api did not listen.{Environment.NewLine}{api.AllOutput}");
        var output = api.StandardOutput;
        var created = output.IndexOf(CreatedMessage, StringComparison.Ordinal);
        var listening = output.IndexOf(ListeningMarker, StringComparison.Ordinal);
        Assert.True(created >= 0, $"The creation message is missing.{Environment.NewLine}{api.AllOutput}");
        Assert.True(created < listening, "The creation message must appear before 'Now listening on'.");

        api.Stop();
        var all = api.AllOutput;
        Assert.DoesNotContain(name, all, StringComparison.Ordinal);
        Assert.DoesNotContain(password, all, StringComparison.Ordinal);

        var (hash, salt, count) = ReadStoredAccount(api.DatabaseFile!);
        Assert.Equal(1, count);
        Assert.NotEmpty(hash);
        foreach (var secret in new[] { hash, salt })
        {
            Assert.DoesNotContain(Convert.ToBase64String(secret), all, StringComparison.Ordinal);
            Assert.DoesNotContain(Convert.ToHexString(secret), all, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task Start_DatabaseFileDoesNotExist_CreatesFileAndAccountAndListens()
    {
        var folder = Path.Combine(Path.GetTempPath(), "shopmanagement-empty-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var databaseFile = Path.Combine(folder, "shop.db");
            Assert.Empty(Directory.GetFileSystemEntries(folder));

            using var api = ApiProcess.Start(
                SetupValues(TestSecrets.NewSignInName(), TestSecrets.NewPassword()),
                databaseFile: databaseFile);

            await api.WaitForExitOrListeningAsync();

            Assert.True(api.Listened, $"The Api did not listen.{Environment.NewLine}{api.AllOutput}");
            Assert.True(File.Exists(databaseFile), "The database file was not created.");

            api.Stop();
            var (_, _, count) = ReadStoredAccount(databaseFile);
            Assert.Equal(1, count);
        }
        finally
        {
            try
            {
                Directory.Delete(folder, recursive: true);
            }
            catch (IOException)
            {
                // Best effort: a leftover temp folder does not affect test results.
            }
        }
    }
}
