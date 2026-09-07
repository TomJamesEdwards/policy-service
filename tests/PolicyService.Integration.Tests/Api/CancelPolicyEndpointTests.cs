using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PolicyService.IntegrationTests.TestData;
using Xunit;

namespace PolicyService.IntegrationTests.Api;

public sealed class CancelPolicyEndpointTests
{
    [Fact]
    public async Task Post_WithValidCancellation_ReturnsAndPersistsCancelledPolicy()
    {
        await using var factory =
            new PolicyServiceApiFactory();

        await factory.InitialiseDatabaseAsync();

        var policy = PolicyTestData.CreateValid();

        await factory.SeedPolicyAsync(policy);

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/policies/{policy.Reference}/cancellation",
            new
            {
                refundReference = "REFUND-000001"
            });

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var cancelledPolicy = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "cancelled",
            cancelledPolicy
                .GetProperty("status")
                .GetString());

        Assert.Equal(
            "2026-01-01",
            cancelledPolicy
                .GetProperty("cancellationDate")
                .GetString());

        var refund = cancelledPolicy
            .GetProperty("refund");

        Assert.Equal(
            "REFUND-000001",
            refund.GetProperty("reference").GetString());

        Assert.Equal(
            "directDebit",
            refund.GetProperty("type").GetString());

        Assert.Equal(
            350.50m,
            refund.GetProperty("amount").GetDecimal());

        var retrievalResponse = await client.GetAsync(
            $"/policies/{policy.Reference}");

        Assert.Equal(
            HttpStatusCode.OK,
            retrievalResponse.StatusCode);

        var retrievedPolicy = await retrievalResponse.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "cancelled",
            retrievedPolicy
                .GetProperty("status")
                .GetString());

        Assert.Equal(
            "REFUND-000001",
            retrievedPolicy
                .GetProperty("refund")
                .GetProperty("reference")
                .GetString());
    }

    [Fact]
    public async Task Post_WhenPolicyDoesNotExist_ReturnsNotFoundProblem()
    {
        await using var factory =
            new PolicyServiceApiFactory();

        await factory.InitialiseDatabaseAsync();

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/policies/HH-2026-999999/cancellation",
            new
            {
                refundReference = "REFUND-000001"
            });

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
    public async Task Post_WhenPolicyIsAlreadyCancelled_ReturnsConflictAndPreservesOriginalRefund()
    {
        await using var factory =
            new PolicyServiceApiFactory();

        await factory.InitialiseDatabaseAsync();

        var policy = PolicyTestData.CreateValid();

        await factory.SeedPolicyAsync(policy);

        using var client = factory.CreateClient();

        var firstResponse = await client.PostAsJsonAsync(
            $"/policies/{policy.Reference}/cancellation",
            new
            {
                refundReference = "REFUND-000001"
            });

        Assert.Equal(
            HttpStatusCode.OK,
            firstResponse.StatusCode);

        var secondResponse = await client.PostAsJsonAsync(
            $"/policies/{policy.Reference}/cancellation",
            new
            {
                refundReference = "REFUND-000002"
            });

        Assert.Equal(
            HttpStatusCode.Conflict,
            secondResponse.StatusCode);

        Assert.Equal(
            "application/problem+json",
            secondResponse.Content.Headers.ContentType?.MediaType);

        var problem = await secondResponse.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "Policy cancellation conflict",
            problem.GetProperty("title").GetString());

        Assert.Equal(
            "policy.already_cancelled",
            problem.GetProperty("code").GetString());

        var retrievalResponse = await client.GetAsync(
            $"/policies/{policy.Reference}");

        Assert.Equal(
            HttpStatusCode.OK,
            retrievalResponse.StatusCode);

        var retrievedPolicy = await retrievalResponse.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "REFUND-000001",
            retrievedPolicy
                .GetProperty("refund")
                .GetProperty("reference")
                .GetString());
    }

    [Fact]
    public async Task Post_WhenRefundIsDueAndReferenceIsMissing_ReturnsBadRequestWithoutCancelling()
    {
        await using var factory =
            new PolicyServiceApiFactory();

        await factory.InitialiseDatabaseAsync();

        var policy = PolicyTestData.CreateValid();

        await factory.SeedPolicyAsync(policy);

        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/policies/{policy.Reference}/cancellation",
            new
            {
                refundReference = (string?)null
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
            "Policy cancellation failed",
            problem.GetProperty("title").GetString());

        Assert.Equal(
            "policy.refund_reference.required",
            problem.GetProperty("code").GetString());

        var retrievalResponse = await client.GetAsync(
            $"/policies/{policy.Reference}");

        Assert.Equal(
            HttpStatusCode.OK,
            retrievalResponse.StatusCode);

        var retrievedPolicy = await retrievalResponse.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "active",
            retrievedPolicy
                .GetProperty("status")
                .GetString());

        Assert.Equal(
            JsonValueKind.Null,
            retrievedPolicy
                .GetProperty("cancellationDate")
                .ValueKind);

        Assert.Equal(
            JsonValueKind.Null,
            retrievedPolicy
                .GetProperty("refund")
                .ValueKind);
    }
}