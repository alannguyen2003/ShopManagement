using System.Net;
using ShopManagement.Api.IntegrationTests.TestSupport;

namespace ShopManagement.Api.IntegrationTests;

public class HealthEndpointsTests
{
    [Fact]
    public async Task GetHealth_AppIsRunning_Returns200()
    {
        using var database = new SqliteTestDatabase();
        using var factory = new ShopApiFactory(database);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
