using Microsoft.Extensions.DependencyInjection;
using ShopManagement.Application.AdminAccounts.SetUpAdminAccount;

namespace ShopManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<SetUpAdminAccountValidator>();
        services.AddScoped<SetUpAdminAccountHandler>();

        return services;
    }
}
