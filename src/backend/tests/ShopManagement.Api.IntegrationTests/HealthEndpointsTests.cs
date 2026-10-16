using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ShopManagement.Api.IntegrationTests;

public class HealthEndpointsTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetHealth_AppIsRunning_Returns200()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
