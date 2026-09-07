using PolicyService.Application.Policies.Renew;
using PolicyService.Application.Tests.Common;
using Xunit;

namespace PolicyService.Application.Tests.Policies.Renew;

public sealed class RenewPolicyHandlerTests
{
    [Fact]
    public async Task Handle_WhenPolicyDoesNotExist_ReturnsNotFoundWithoutSaving()
    {
        var repository = new StubPolicyRepository();

        var handler = new RenewPolicyHandler(
            repository,
            TimeProvider.System);

        var command = new RenewPolicyCommand(
            Reference: "HH-2026-999999",
            PaymentReference: "PAY-000002",
            CardNumber: null);

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.False(
            repository.SaveChangesWasCalled);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "policy.not_found",
            error.Code);
    }

    [Fact]
    public async Task Handle_WhenRenewalIsValid_RenewsAndSavesPolicy()
    {
        var policy = PolicyTestData.CreateValid();

        var repository = new StubPolicyRepository(
            policy: policy);

        var timeProvider = new FixedTimeProvider(
            new DateTimeOffset(2027, 1, 1, 0, 0, 0,
                TimeSpan.Zero));

        var handler = new RenewPolicyHandler(
            repository,
            timeProvider);

        var command = new RenewPolicyCommand(
            Reference: policy.Reference,
            PaymentReference: "PAY-000002",
            CardNumber: null);

        var originalEndDate = policy.EndDate;

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(
            repository.SaveChangesWasCalled);

        Assert.Same(
            policy,
            result.Value);

        Assert.Equal(
            originalEndDate.AddYears(1),
            result.Value.EndDate);

        Assert.Equal(
            2,
            result.Value.Payments.Count);

        Assert.Equal(
            "PAY-000002",
            result.Value.Payments[^1].Reference);
    }

    [Fact]
    public async Task Handle_WhenRenewalIsTooEarly_ReturnsFailureWithoutSaving()
    {
        var policy = PolicyTestData.CreateValid();

        var repository = new StubPolicyRepository(
            policy: policy);

        var timeProvider = new FixedTimeProvider(
            new DateTimeOffset(
                2026, 12, 31, 0, 0, 0,
                TimeSpan.Zero));

        var handler = new RenewPolicyHandler(
            repository,
            timeProvider);

        var command = new RenewPolicyCommand(
            Reference: policy.Reference,
            PaymentReference: "PAY-000002",
            CardNumber: null);

        var originalEndDate = policy.EndDate;

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.False(
            repository.SaveChangesWasCalled);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "policy.renewal.too_early",
            error.Code);

        Assert.Equal(
            originalEndDate,
            policy.EndDate);

        Assert.Single(policy.Payments);
    }
}