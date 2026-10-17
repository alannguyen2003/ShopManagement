using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ShopManagement.Infrastructure.Persistence;

namespace ShopManagement.Api.IntegrationTests.AdminAccounts;

public sealed class AdminAccountSchemaTests : IDisposable
{
    private const int PrimaryKeyViolation = 1555;

    private readonly SqliteConnection _connection;

    public AdminAccountSchemaTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var context = new AppDbContext(options);
        context.Database.Migrate();
    }

    public void Dispose() => _connection.Dispose();

    private int Insert(
        int id,
        string? signInName = "admin",
        string? algorithm = "PBKDF2-HMAC-SHA256",
        byte[]? salt = null,
        int? iterations = 600_000,
        byte[]? hash = null,
        bool useDefaultBlobs = true)
    {
        using var command = _connection.CreateCommand();
        command.CommandText =
            "INSERT INTO AdminAccounts (Id, SignInName, PasswordAlgorithm, PasswordSalt, PasswordIterations, PasswordHash) " +
            "VALUES ($id, $name, $algorithm, $salt, $iterations, $hash)";
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$name", (object?)signInName ?? DBNull.Value);
        command.Parameters.AddWithValue("$algorithm", (object?)algorithm ?? DBNull.Value);
        command.Parameters.AddWithValue("$salt", (object?)(salt ?? (useDefaultBlobs ? new byte[16] : null)) ?? DBNull.Value);
        command.Parameters.AddWithValue("$iterations", (object?)iterations ?? DBNull.Value);
        command.Parameters.AddWithValue("$hash", (object?)(hash ?? (useDefaultBlobs ? new byte[32] : null)) ?? DBNull.Value);
        return command.ExecuteNonQuery();
    }

    [Fact]
    public void Insert_IdOne_Succeeds()
    {
        Assert.Equal(1, Insert(1));
    }

    [Fact]
    public void Insert_IdTwo_FailsOnTheSingleRowCheck()
    {
        var exception = Assert.Throws<SqliteException>(() => Insert(2));

        Assert.Contains("CK_AdminAccounts_SingleRow", exception.Message);
    }

    [Fact]
    public void Insert_SecondRowWithIdOne_FailsWithPrimaryKeyViolation()
    {
        Insert(1);

        var exception = Assert.Throws<SqliteException>(() => Insert(1));

        Assert.Equal(PrimaryKeyViolation, exception.SqliteExtendedErrorCode);
    }

    [Fact]
    public void Insert_NullSignInName_Fails()
    {
        Assert.Throws<SqliteException>(() => Insert(1, signInName: null));
    }

    [Fact]
    public void Insert_NullPasswordAlgorithm_Fails()
    {
        Assert.Throws<SqliteException>(() => Insert(1, algorithm: null));
    }

    [Fact]
    public void Insert_NullPasswordSalt_Fails()
    {
        Assert.Throws<SqliteException>(() => Insert(1, salt: null, useDefaultBlobs: false, hash: new byte[32]));
    }

    [Fact]
    public void Insert_NullPasswordIterations_Fails()
    {
        Assert.Throws<SqliteException>(() => Insert(1, iterations: null));
    }

    [Fact]
    public void Insert_NullPasswordHash_Fails()
    {
        Assert.Throws<SqliteException>(() => Insert(1, hash: null, useDefaultBlobs: false, salt: new byte[16]));
    }
}
