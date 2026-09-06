using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PolicyService.IntegrationTests.TestData;
using Xunit;

namespace PolicyService.IntegrationTests.Api;

public sealed class GetPolicyEndpointTests
{
    [Fact]
    public async Task Get_WhenPolicyDoesNotExist_ReturnsNotFoundProblem()
    {
        await using var factory =
            new PolicyServiceApiFactory();

        await factory.InitialiseDatabaseAsync();

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/policies/HH-DOES-NOT-EXIST");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "Policy not found",
            problem.GetProperty("title").GetString());

        Assert.Equal(
            "policy.not_found",
            problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Get_WhenPolicyExists_ReturnsCompletePolicy()
    {
        await using var factory =
            new PolicyServiceApiFactory();

        await factory.InitialiseDatabaseAsync();

        var policy = PolicyTestData.CreateValid();

        await factory.SeedPolicyAsync(policy);

        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"/policies/{policy.Reference}");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var content = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            policy.Reference,
            content.GetProperty("reference").GetString());

        Assert.Equal(
            "household",
            content.GetProperty("type").GetString());

        Assert.Equal(
            "2026-02-01",
            content.GetProperty("startDate").GetString());

        Assert.Equal(
            "2027-01-31",
            content.GetProperty("endDate").GetString());

        var policyholders =
            content.GetProperty("policyholders");

        Assert.Equal(1, policyholders.GetArrayLength());

        Assert.Equal(
            "Anne",
            policyholders[0]
                .GetProperty("firstName")
                .GetString());

        Assert.Equal(
            "CH7 1AA",
            content
                .GetProperty("property")
                .GetProperty("postcode")
                .GetString());

        var payments = content.GetProperty("payments");

        Assert.Equal(1, payments.GetArrayLength());

        Assert.Equal(
            "directDebit",
            payments[0]
                .GetProperty("type")
                .GetString());

        Assert.False(
            payments[0].TryGetProperty(
                "cardNumber",
                out _));
    }

}