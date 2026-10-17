using Microsoft.Extensions.DependencyInjection;
using ShopManagement.Application.AdminAccounts.SetUpAdminAccount;

namespace ShopManagement.Application.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_OnServiceCollection_ReturnsSameCollection()
    {
        var services = new ServiceCollection();

        var returned = services.AddApplication();

        Assert.Same(services, returned);
    }

    [Theory]
    [InlineData(typeof(SetUpAdminAccountHandler))]
    [InlineData(typeof(SetUpAdminAccountValidator))]
    public void AddApplication_OnServiceCollection_RegistersSetUpTypesAsScoped(Type serviceType)
    {
        var services = new ServiceCollection();

        services.AddApplication();

        var descriptor = Assert.Single(services, d => d.ServiceType == serviceType);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }
}
