using System.Net;
using Xunit;

namespace PolicyService.IntegrationTests.Api;

public sealed class HealthEndpointTests
{
    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task Get_WhenApplicationIsReady_ReturnsHealthy(
        string endpoint)
    {
        await using var factory =
            new PolicyServiceApiFactory();

        await factory.InitialiseDatabaseAsync();

        using var client = factory.CreateClient();

        var response = await client.GetAsync(endpoint);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var content =
            await response.Content.ReadAsStringAsync();

        Assert.Equal("Healthy", content);
    }
}