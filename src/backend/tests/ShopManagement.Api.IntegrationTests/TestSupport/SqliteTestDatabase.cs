using Microsoft.Data.Sqlite;

namespace ShopManagement.Api.IntegrationTests.TestSupport;

// One in-memory SQLite database per test. The sentinel connection keeps it alive while the test runs.
public sealed class SqliteTestDatabase : IDisposable
{
    private readonly SqliteConnection _sentinel;

    public SqliteTestDatabase()
    {
        ConnectionString = $"Data Source={Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        _sentinel = new SqliteConnection(ConnectionString);
        _sentinel.Open();
    }

    public string ConnectionString { get; }

    public void Dispose() => _sentinel.Dispose();
}
