using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopManagement.Application.AdminAccounts;
using ShopManagement.Application.Common;
using ShopManagement.Infrastructure.Persistence;
using ShopManagement.Infrastructure.Persistence.Repositories;
using ShopManagement.Infrastructure.Security;

namespace ShopManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        // The connection string is read when the context is resolved so resolving never throws;
        // a missing value is reported by DatabaseMigrator as ConnectionStringMissing.
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var connectionString = serviceProvider.GetRequiredService<IConfiguration>()
                .GetConnectionString("Default");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseSqlite();
            }
            else
            {
                options.UseSqlite(connectionString);
            }
        });

        services.AddScoped<IDatabaseMigrator>(serviceProvider => new DatabaseMigrator(
            serviceProvider.GetRequiredService<IConfiguration>(),
            serviceProvider.GetRequiredService<AppDbContext>(),
            DatabaseMigrator.DefaultLockTimeout));
        services.AddScoped<IAdminAccountRepository, AdminAccountRepository>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
