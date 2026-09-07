using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PolicyService.IntegrationTests.TestData;
using Xunit;

namespace PolicyService.IntegrationTests.Api;

public sealed class RenewPolicyEndpointTests
{
    [Fact]
    public async Task Post_WithValidRenewal_ReturnsAndPersistsRenewedPolicy()
    {
        await using var factory =
            new PolicyServiceApiFactory(
                new DateTimeOffset(2027, 1, 1, 0, 0, 0,
                TimeSpan.Zero));

        await factory.InitialiseDatabaseAsync();

        var policy = PolicyTestData.CreateValid();

        await factory.SeedPolicyAsync(policy);

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/policies/{policy.Reference}/renewal",
            new
            {
                paymentReference = "PAY-000002",
                cardNumber = (string?)null
            });

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var renewedPolicy = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "2028-01-31",
            renewedPolicy
                .GetProperty("endDate")
                .GetString());

        var payments = renewedPolicy
            .GetProperty("payments");

        Assert.Equal(
            2,
            payments.GetArrayLength());

        Assert.Equal(
            "PAY-000002",
            payments[1]
                .GetProperty("reference")
                .GetString());

        var retrievalResponse = await client.GetAsync(
            $"/policies/{policy.Reference}");

        Assert.Equal(
            HttpStatusCode.OK,
            retrievalResponse.StatusCode);

        var retrievedPolicy = await retrievalResponse.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "2028-01-31",
            retrievedPolicy
                .GetProperty("endDate")
                .GetString());

        Assert.Equal(
            2,
            retrievedPolicy
                .GetProperty("payments")
                .GetArrayLength());
    }

    [Fact]
    public async Task Post_WhenRenewalIsTooEarly_ReturnsBadRequestWithoutChangingPolicy()
    {
        await using var factory =
            new PolicyServiceApiFactory();

        await factory.InitialiseDatabaseAsync();

        var policy = PolicyTestData.CreateValid();

        await factory.SeedPolicyAsync(policy);

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/policies/{policy.Reference}/renewal",
            new
            {
                paymentReference = "PAY-000002",
                cardNumber = (string?)null
            });

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "Policy renewal failed",
            problem.GetProperty("title").GetString());

        Assert.Equal(
            "policy.renewal.too_early",
            problem.GetProperty("code").GetString());

        var retrievalResponse = await client.GetAsync(
            $"/policies/{policy.Reference}");

        Assert.Equal(
            HttpStatusCode.OK,
            retrievalResponse.StatusCode);

        var retrievedPolicy = await retrievalResponse.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "2027-01-31",
            retrievedPolicy
                .GetProperty("endDate")
                .GetString());

        Assert.Equal(
            1,
            retrievedPolicy
                .GetProperty("payments")
                .GetArrayLength());
    }

    [Fact]
    public async Task Post_WhenPolicyDoesNotExist_ReturnsNotFoundProblem()
    {
        await using var factory =
            new PolicyServiceApiFactory(
                new DateTimeOffset(
                    2027, 1, 1, 0, 0, 0,
                    TimeSpan.Zero));

        await factory.InitialiseDatabaseAsync();

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/policies/HH-2026-999999/renewal",
            new
            {
                paymentReference = "PAY-000002",
                cardNumber = (string?)null
            });

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

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
    public async Task Post_WhenPolicyIsCancelled_ReturnsConflict()
    {
        await using var factory =
            new PolicyServiceApiFactory(
                new DateTimeOffset(
                    2027, 1, 1, 0, 0, 0,
                    TimeSpan.Zero));

        await factory.InitialiseDatabaseAsync();

        var policy = PolicyTestData.CreateValid();

        var cancellationResult = policy.Cancel(
            policy.StartDate.AddDays(-1),
            refundReference: "REFUND-000001");

        Assert.True(cancellationResult.IsSuccess);

        await factory.SeedPolicyAsync(policy);

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/policies/{policy.Reference}/renewal",
            new
            {
                paymentReference = "PAY-000002",
                cardNumber = (string?)null
            });

        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);

        var problem = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "Policy renewal conflict",
            problem.GetProperty("title").GetString());

        Assert.Equal(
            "policy.renewal.cancelled",
            problem.GetProperty("code").GetString());
    }
}