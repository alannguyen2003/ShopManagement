using Microsoft.Extensions.DependencyInjection;

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
}
