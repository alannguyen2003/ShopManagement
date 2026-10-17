using System.Security.Cryptography;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ShopManagement.Application.AdminAccounts;
using ShopManagement.Application.Common;
using ShopManagement.Domain.AdminAccounts;
using ShopManagement.Domain.Common;
using ShopManagement.Infrastructure.Persistence;
using ShopManagement.Infrastructure.Persistence.Repositories;

namespace ShopManagement.Api.IntegrationTests.AdminAccounts;

public sealed class AdminAccountRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SaveChangesCounter _counter = new();

    public AdminAccountRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var context = NewContext();
        context.Database.Migrate();
    }

    public void Dispose() => _connection.Dispose();

    private AppDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(_counter)
            .Options;
        return new AppDbContext(options);
    }

    private static StoredPassword NewStoredPassword() =>
        new(
            StoredPassword.Pbkdf2HmacSha256,
            RandomNumberGenerator.GetBytes(StoredPassword.MinimumSaltSize),
            StoredPassword.MinimumIterations,
            RandomNumberGenerator.GetBytes(StoredPassword.HashSize));

    private static string NewName() => "name-" + Guid.NewGuid().ToString("N");

    private async Task<AddAdminAccountOutcome> AddAsync(AdminAccount account)
    {
        using var context = NewContext();
        return await new AdminAccountRepository(context).AddAsync(account, CancellationToken.None);
    }

    private async Task<Result<IReadOnlyList<AdminAccount>>> ListAsync()
    {
        using var context = NewContext();
        return await new AdminAccountRepository(context).ListAsync(CancellationToken.None);
    }

    private void Execute(string sql, Action<SqliteCommand>? bind = null)
    {
        using var command = _connection.CreateCommand();
        command.CommandText = sql;
        bind?.Invoke(command);
        command.ExecuteNonQuery();
    }

    private void RebuildTableWithoutNotNull() =>
        Execute(
            "DROP TABLE AdminAccounts; " +
            "CREATE TABLE AdminAccounts (Id INTEGER NOT NULL PRIMARY KEY, SignInName TEXT NULL, " +
            "PasswordAlgorithm TEXT NULL, PasswordSalt BLOB NULL, PasswordIterations INTEGER NULL, " +
            "PasswordHash BLOB NULL, CONSTRAINT CK_AdminAccounts_SingleRow CHECK (\"Id\" = 1))");

    [Fact]
    public async Task ListAsync_EmptyTable_ReturnsSuccessWithNoAccount()
    {
        var result = await ListAsync();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task AddAsync_NewAccount_ReturnsAddedAndListReturnsNameAndPasswordFields()
    {
        var name = NewName();
        var password = NewStoredPassword();

        var outcome = await AddAsync(AdminAccount.Create(name, password));
        var result = await ListAsync();

        Assert.Equal(AddAdminAccountOutcome.Added, outcome);
        Assert.True(result.IsSuccess);
        var account = Assert.Single(result.Value);
        Assert.Equal(AdminAccount.SingleId, account.Id);
        Assert.Equal(name, account.SignInName);
        Assert.Equal(password.Algorithm, account.Password.Algorithm);
        Assert.Equal(password.Salt, account.Password.Salt);
        Assert.Equal(password.Iterations, account.Password.Iterations);
        Assert.Equal(password.Hash, account.Password.Hash);
        Assert.True(account.Password.IsUsable);
    }

    [Fact]
    public async Task AddAsync_NewAccount_SavesExactlyOnce()
    {
        _counter.Reset();

        await AddAsync(AdminAccount.Create(NewName(), NewStoredPassword()));

        Assert.Equal(1, _counter.Count);
    }

    [Fact]
    public async Task AddAsync_RowAlreadyExists_ReturnsAlreadyExistsAndKeepsStoredRow()
    {
        var firstName = NewName();
        var firstPassword = NewStoredPassword();
        await AddAsync(AdminAccount.Create(firstName, firstPassword));

        var outcome = await AddAsync(AdminAccount.Create(NewName(), NewStoredPassword()));
        var result = await ListAsync();

        Assert.Equal(AddAdminAccountOutcome.AlreadyExists, outcome);
        var account = Assert.Single(result.Value);
        Assert.Equal(firstName, account.SignInName);
        Assert.Equal(firstPassword.Salt, account.Password.Salt);
        Assert.Equal(firstPassword.Hash, account.Password.Hash);
    }

    [Fact]
    public async Task ListAsync_PasswordColumnsNull_ReturnsAccountWithUnusablePassword()
    {
        RebuildTableWithoutNotNull();
        var name = NewName();
        Execute(
            "INSERT INTO AdminAccounts (Id, SignInName) VALUES (1, $name)",
            command => command.Parameters.AddWithValue("$name", name));

        var result = await ListAsync();

        Assert.True(result.IsSuccess);
        var account = Assert.Single(result.Value);
        Assert.Equal(name, account.SignInName);
        Assert.False(account.Password.IsUsable);
    }

    [Fact]
    public async Task ListAsync_IterationsStoredAsText_ReturnsAccountWithUnusablePassword()
    {
        RebuildTableWithoutNotNull();
        Execute(
            "INSERT INTO AdminAccounts (Id, SignInName, PasswordAlgorithm, PasswordSalt, PasswordIterations, PasswordHash) " +
            "VALUES (1, $name, $algorithm, $salt, $iterations, $hash)",
            command =>
            {
                command.Parameters.AddWithValue("$name", NewName());
                command.Parameters.AddWithValue("$algorithm", StoredPassword.Pbkdf2HmacSha256);
                command.Parameters.AddWithValue("$salt", RandomNumberGenerator.GetBytes(StoredPassword.MinimumSaltSize));
                command.Parameters.AddWithValue("$iterations", "not-a-number");
                command.Parameters.AddWithValue("$hash", RandomNumberGenerator.GetBytes(StoredPassword.HashSize));
            });

        var result = await ListAsync();

        Assert.True(result.IsSuccess);
        var account = Assert.Single(result.Value);
        Assert.False(account.Password.IsUsable);
    }

    [Fact]
    public async Task ListAsync_SignInNameNull_ReturnsUnreadableStoredDataFailure()
    {
        RebuildTableWithoutNotNull();
        Execute(
            "INSERT INTO AdminAccounts (Id, SignInName, PasswordAlgorithm, PasswordSalt, PasswordIterations, PasswordHash) " +
            "VALUES (1, NULL, $algorithm, $salt, $iterations, $hash)",
            command =>
            {
                command.Parameters.AddWithValue("$algorithm", StoredPassword.Pbkdf2HmacSha256);
                command.Parameters.AddWithValue("$salt", RandomNumberGenerator.GetBytes(StoredPassword.MinimumSaltSize));
                command.Parameters.AddWithValue("$iterations", StoredPassword.MinimumIterations);
                command.Parameters.AddWithValue("$hash", RandomNumberGenerator.GetBytes(StoredPassword.HashSize));
            });

        var result = await ListAsync();

        Assert.True(result.IsFailure);
        Assert.Equal(StoredDataErrors.UnreadableStoredData, result.Error);
        Assert.Equal("StoredData.Unreadable", result.Error.Code);
    }

    private sealed class SaveChangesCounter : SaveChangesInterceptor
    {
        private int _count;

        public int Count => _count;

        public void Reset() => Interlocked.Exchange(ref _count, 0);

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _count);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
