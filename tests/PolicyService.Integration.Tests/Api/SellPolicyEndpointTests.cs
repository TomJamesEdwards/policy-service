using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace PolicyService.IntegrationTests.Api;

public sealed class SellPolicyEndpointTests
{
    private static object CreateRequest(
        string reference = "HH-2026-000001",
        string type = "household",
        decimal amount = 350.50m,
        string paymentType = "directDebit",
        string? cardNumber = null)
    {
        return new
        {
            reference,
            type,
            startDate = "2026-02-01",
            amount,
            autoRenew = true,
            policyholders = new[]
               {
                new
                {
                    firstName = "Anne",
                    lastName = "Example",
                    dateOfBirth = "1990-04-12"
                }
            },
            property = new
            {
                addressLine1 = "1 Test Street",
                addressLine2 = (string?)null,
                addressLine3 = (string?)null,
                postcode = "CH7 1AA"
            },
            payment = new
            {
                reference = "PAY-000001",
                type = paymentType,
                cardNumber
            }
        };

    }

    [Theory]
    [InlineData("household", "HH-2026-000001")]
    [InlineData("buyToLet", "BTL-2026-000001")]
    public async Task Post_WithSupportedPolicyType_ReturnsCreatedPolicyThatCanBeRetrieved(
        string type,
        string reference)
    {
        await using var factory = new PolicyServiceApiFactory();

        await factory.InitialiseDatabaseAsync();

        using var client = factory.CreateClient();

        var request = CreateRequest(
            reference: reference,
            type: type);

        var response = await client.PostAsJsonAsync(
            "/policies",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var location = response.Headers.Location;

        Assert.NotNull(location);

        Assert.Equal(
            $"/policies/{reference}",
            location.AbsolutePath);

        var createdPolicy = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            reference,
            createdPolicy.GetProperty("reference").GetString());

        Assert.Equal(
            type,
            createdPolicy.GetProperty("type").GetString());

        var retrievalResponse = await client.GetAsync(location);

        Assert.Equal(
            HttpStatusCode.OK,
            retrievalResponse.StatusCode);

        var retrievedPolicy = await retrievalResponse.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            reference,
            retrievedPolicy.GetProperty("reference").GetString());

        Assert.Equal(
            type,
            retrievedPolicy.GetProperty("type").GetString());
    }

    [Fact]
    public async Task Post_WhenReferenceAlreadyExists_ReturnsConflictProblem()
    {
        await using var factory =
            new PolicyServiceApiFactory();

        await factory.InitialiseDatabaseAsync();

        using var client = factory.CreateClient();

        var request = CreateRequest();

        var firstResponse = await client.PostAsJsonAsync(
            "/policies",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);

        var duplicateResponse = await client.PostAsJsonAsync(
            "/policies",
            request);

        Assert.Equal(
            HttpStatusCode.Conflict,
            duplicateResponse.StatusCode);

        Assert.Equal(
            "application/problem+json",
            duplicateResponse.Content.Headers.ContentType?.MediaType);

        var problem = await duplicateResponse.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "Policy reference conflict",
            problem.GetProperty("title").GetString());

        Assert.Equal(
            "policy.reference.conflict",
            problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Post_WithInvalidAmount_ReturnsValidationProblemWithoutSaving()
    {
        await using var factory =
            new PolicyServiceApiFactory();

        await factory.InitialiseDatabaseAsync();

        using var client = factory.CreateClient();

        var request = CreateRequest(amount: 0m);

        var response = await client.PostAsJsonAsync(
            "/policies",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "Policy validation failed",
            problem.GetProperty("title").GetString());

        Assert.Equal(
            "payment.amount.must_be_positive",
            problem.GetProperty("code").GetString());

        var retrievalResponse = await client.GetAsync(
            "/policies/HH-2026-000001");

        Assert.Equal(
            HttpStatusCode.NotFound,
            retrievalResponse.StatusCode);
    }

    [Fact]
    public async Task Post_WithInvalidCardNumber_ReturnsValidationProblemWithoutSaving()
    {
        await using var factory =
            new PolicyServiceApiFactory();

        await factory.InitialiseDatabaseAsync();

        using var client = factory.CreateClient();

        var request = CreateRequest(
            paymentType: "card",
            cardNumber: "4111111111111112");

        var response = await client.PostAsJsonAsync(
            "/policies",
            request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problem = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "payment.card_number.invalid",
            problem.GetProperty("code").GetString());

        var retrievalResponse = await client.GetAsync(
            "/policies/HH-2026-000001");

        Assert.Equal(
            HttpStatusCode.NotFound,
            retrievalResponse.StatusCode);
    }

    [Fact]
    public async Task Post_WithValidCardNumber_DoesNotExposeCardNumber()
    {
        const string cardNumber = "4111111111111111";

        await using var factory =
            new PolicyServiceApiFactory();

        await factory.InitialiseDatabaseAsync();

        using var client = factory.CreateClient();

        var request = CreateRequest(
            paymentType: "card",
            cardNumber: cardNumber);

        var response = await client.PostAsJsonAsync(
            "/policies",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var createdJson =
            await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain(cardNumber, createdJson);

        using (var document = JsonDocument.Parse(createdJson))
        {
            var payment = document.RootElement
                .GetProperty("payments")[0];

            Assert.Equal(
                "card",
                payment.GetProperty("type").GetString());

            Assert.False(
                payment.TryGetProperty(
                    "cardNumber",
                    out _));
        }

        var location = response.Headers.Location;

        Assert.NotNull(location);

        var retrievalResponse =
            await client.GetAsync(location);

        Assert.Equal(
            HttpStatusCode.OK,
            retrievalResponse.StatusCode);

        var retrievedJson =
            await retrievalResponse.Content.ReadAsStringAsync();

        Assert.DoesNotContain(cardNumber, retrievedJson);

        using var retrievedDocument =
            JsonDocument.Parse(retrievedJson);

        var retrievedPayment = retrievedDocument.RootElement
            .GetProperty("payments")[0];

        Assert.False(
            retrievedPayment.TryGetProperty(
                "cardNumber",
                out _));
    }

}