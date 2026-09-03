using PolicyService.Application.Abstractions.Persistence;
using PolicyService.Application.Policies.GetByReference;
using PolicyService.Domain.Policies;
using Xunit;

namespace PolicyService.Application.Tests.Policies.GetByReference;

public sealed class GetPolicyByReferenceHandlerTests
{
    private sealed class StubPolicyRepository : IPolicyRepository
    {
        private readonly Policy? _policy;
        internal StubPolicyRepository(Policy? policy)
        {
            _policy = policy;
        }
        public Task<Policy?> GetByReferenceAsync(
    string reference,
    CancellationToken cancellationToken)
        {
            return Task.FromResult(_policy);
        }
    }
    private static Policy CreatePolicy()
    {
        var property = InsuredProperty.Create(
            addressLine1: "1 Test Street",
            addressLine2: null,
            addressLine3: null,
            postcode: "CH7 1AA").Value;

        return Policy.Sell(
            reference: "HH-2026-000001",
            type: PolicyType.Household,
            startDate: new DateOnly(2026, 2, 1),
            amount: 350.50m,
            autoRenew: true,
            hasClaims: false,
            policyholders:
            [
                new Policyholder(
                    "Anne",
                    "Example",
                    new DateOnly(1990, 4, 12))
            ],
            property,
            paymentReference: "PAY-000001",
            paymentType: PaymentType.DirectDebit,
            today: new DateOnly(2026, 1, 1)).Value;
    }


    [Fact]
    public async Task Handle_WhenPolicyDoesNotExist_ReturnsNotFoundFailure()
    {
        var repository = new StubPolicyRepository(policy: null);
        var handler = new GetPolicyByReferenceHandler(repository);
        var query = new GetPolicyByReferenceQuery("POL-000001");

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal("policy.not_found", error.Code);
        Assert.Equal("Policy 'POL-000001' was not found.", error.Message);
    }

    [Fact]
    public async Task Handle_WhenPolicyExists_ReturnsPolicy()
    {
        var policy = CreatePolicy();
        var repository = new StubPolicyRepository(policy);
        var handler = new GetPolicyByReferenceHandler(repository);
        var query = new GetPolicyByReferenceQuery("HH-2026-000001");

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Same(policy, result.Value);
    }

}