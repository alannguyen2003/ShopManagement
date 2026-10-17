using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ShopManagement.Application.AdminAccounts.SetUpAdminAccount;

namespace ShopManagement.Api.IntegrationTests.TestSupport;

public sealed class ShopApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteTestDatabase _database;
    private readonly string? _signInName;
    private readonly string? _initialPassword;

    public ShopApiFactory(SqliteTestDatabase database)
        : this(database, TestSecrets.NewSignInName(), TestSecrets.NewPassword())
    {
    }

    // A null value means the setting is absent.
    public ShopApiFactory(SqliteTestDatabase database, string? signInName, string? initialPassword)
    {
        _database = database;
        _signInName = signInName;
        _initialPassword = initialPassword;
    }

    public CapturingLoggerProvider Logs { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", _database.ConnectionString);

        builder.ConfigureLogging(logging => logging.AddProvider(Logs));

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<SetUpAdminAccountCommand>();
            services.AddSingleton(new SetUpAdminAccountCommand(_signInName, _initialPassword));
        });
    }
}
