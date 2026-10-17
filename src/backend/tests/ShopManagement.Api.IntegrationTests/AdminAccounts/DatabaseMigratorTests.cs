using System.Diagnostics;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShopManagement.Application.Common;
using ShopManagement.Infrastructure.Persistence;

namespace ShopManagement.Api.IntegrationTests.AdminAccounts;

public sealed class DatabaseMigratorTests : IDisposable
{
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(10);

    private readonly string _folder = Path.Combine(Path.GetTempPath(), "shop-migrator-" + Guid.NewGuid().ToString("N"));

    public DatabaseMigratorTests() => Directory.CreateDirectory(_folder);

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_folder))
        {
            Directory.Delete(_folder, recursive: true);
        }
    }

    private string NewDatabasePath() => Path.Combine(_folder, Guid.NewGuid().ToString("N") + ".db");

    private static DatabaseMigrator BuildMigrator(string? connectionString, TimeSpan lockTimeout, out AppDbContext context)
    {
        var settings = new Dictionary<string, string?>();
        if (connectionString is not null)
        {
            settings["ConnectionStrings:Default"] = connectionString;
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        // As in the composition root: the connection string is read when the context is built, and absent means no value.
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        var configured = configuration.GetConnectionString("Default");
        if (string.IsNullOrEmpty(configured))
        {
            builder.UseSqlite();
        }
        else
        {
            builder.UseSqlite(configured);
        }

        context = new AppDbContext(builder.Options);
        return new DatabaseMigrator(configuration, context, lockTimeout);
    }

    private static bool TableExists(string path, string table)
    {
        using var connection = new SqliteConnection("Data Source=" + path + ";Pooling=False");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name";
        command.Parameters.AddWithValue("$name", table);
        return (long)command.ExecuteScalar()! == 1;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task MigrateAsync_ConnectionStringAbsentOrEmpty_FailsWithConnectionStringMissing(string? connectionString)
    {
        var migrator = BuildMigrator(connectionString, TimeSpan.FromSeconds(2), out var context);
        using (context)
        {
            var result = await migrator.MigrateAsync(CancellationToken.None).WaitAsync(TestTimeout);

            Assert.True(result.IsFailure);
            Assert.Equal(StoredDataErrors.ConnectionStringMissing, result.Error);
            Assert.Equal("StoredData.ConnectionStringMissing", result.Error.Code);
        }
    }

    [Fact]
    public async Task MigrateAsync_FileDoesNotExist_CreatesFileWithAdminAccountsTable()
    {
        var path = NewDatabasePath();
        var migrator = BuildMigrator("Data Source=" + path, TimeSpan.FromSeconds(30), out var context);
        using (context)
        {
            Assert.False(File.Exists(path));

            var result = await migrator.MigrateAsync(CancellationToken.None).WaitAsync(TestTimeout);

            Assert.True(result.IsSuccess);
        }

        SqliteConnection.ClearAllPools();
        Assert.True(File.Exists(path));
        Assert.True(TableExists(path, "AdminAccounts"));
    }

    [Fact]
    public async Task MigrateAsync_ExistingEmptyFile_MigratesAndCreatesTable()
    {
        var path = NewDatabasePath();
        await File.WriteAllBytesAsync(path, []);
        var migrator = BuildMigrator("Data Source=" + path, TimeSpan.FromSeconds(30), out var context);
        using (context)
        {
            var result = await migrator.MigrateAsync(CancellationToken.None).WaitAsync(TestTimeout);

            Assert.True(result.IsSuccess);
        }

        SqliteConnection.ClearAllPools();
        Assert.True(TableExists(path, "AdminAccounts"));
    }

    [Fact]
    public async Task MigrateAsync_AlreadyMigrated_Succeeds()
    {
        var path = NewDatabasePath();
        var first = BuildMigrator("Data Source=" + path, TimeSpan.FromSeconds(30), out var firstContext);
        using (firstContext)
        {
            await first.MigrateAsync(CancellationToken.None).WaitAsync(TestTimeout);
        }

        var second = BuildMigrator("Data Source=" + path, TimeSpan.FromSeconds(30), out var secondContext);
        using (secondContext)
        {
            var result = await second.MigrateAsync(CancellationToken.None).WaitAsync(TestTimeout);

            Assert.True(result.IsSuccess);
        }
    }

    [Fact]
    public async Task MigrateAsync_MigrationLockHeld_FailsWithStoredDataLockedWithinTheTimeout()
    {
        var path = NewDatabasePath();
        using (var connection = new SqliteConnection("Data Source=" + path + ";Pooling=False"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText =
                "CREATE TABLE IF NOT EXISTS \"__EFMigrationsLock\" (\"Id\" INTEGER NOT NULL CONSTRAINT \"PK___EFMigrationsLock\" PRIMARY KEY, \"Timestamp\" TEXT NOT NULL); " +
                "INSERT INTO \"__EFMigrationsLock\" (\"Id\", \"Timestamp\") VALUES (1, $timestamp)";
            command.Parameters.AddWithValue("$timestamp", DateTimeOffset.UtcNow.ToString("O"));
            command.ExecuteNonQuery();
        }

        var lockTimeout = TimeSpan.FromSeconds(2);
        var migrator = BuildMigrator("Data Source=" + path, lockTimeout, out var context);
        using (context)
        {
            var stopwatch = Stopwatch.StartNew();

            var result = await migrator.MigrateAsync(CancellationToken.None).WaitAsync(TestTimeout);

            stopwatch.Stop();
            Assert.True(result.IsFailure);
            Assert.Equal(StoredDataErrors.StoredDataLocked, result.Error);
            Assert.Equal("StoredData.Locked", result.Error.Code);
            Assert.True(stopwatch.Elapsed < TestTimeout, "The migrator must stop waiting for the lock.");
            Assert.True(stopwatch.Elapsed >= lockTimeout - TimeSpan.FromMilliseconds(200), "The migrator must wait for the timeout.");
        }
    }
}
