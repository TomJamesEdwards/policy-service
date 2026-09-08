using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PolicyService.Application.Abstractions.Persistence;
using Xunit;

namespace PolicyService.IntegrationTests.Api;

public sealed class UnhandledExceptionEndpointTests
{
    private const string ExceptionMessage =
        "This exception must not be returned to the client.";

    [Fact]
    public async Task Request_WhenUnhandledExceptionOccurs_ReturnsSafeProblem()
    {
        await using var factory =
            new PolicyServiceApiFactory();

        await using var faultingFactory =
            factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IPolicyRepository>();

                    services.AddScoped<IPolicyRepository>(_ =>
                        throw new InvalidOperationException(
                            ExceptionMessage));
                });
            });

        using var client = faultingFactory.CreateClient();

        var response = await client.GetAsync(
            "/policies/HH-EXCEPTION");

        Assert.Equal(
            HttpStatusCode.InternalServerError,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var content = await response.Content
            .ReadAsStringAsync();

        using var document = JsonDocument.Parse(content);

        var problem = document.RootElement;

        Assert.Equal(
            "An unexpected error occurred",
            problem.GetProperty("title").GetString());

        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            problem.GetProperty("status").GetInt32());

        Assert.Equal(
            "server.unexpected_error",
            problem.GetProperty("code").GetString());

        Assert.False(
            string.IsNullOrWhiteSpace(
                problem.GetProperty("traceId").GetString()));

        Assert.DoesNotContain(
            ExceptionMessage,
            content);

        Assert.DoesNotContain(
            nameof(InvalidOperationException),
            content);
    }
}