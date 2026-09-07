using PolicyService.Application.Policies.Cancel;
using PolicyService.Application.Tests.Common;
using PolicyService.Domain.Policies;
using Xunit;

namespace PolicyService.Application.Tests.Policies.Cancel;

public sealed class CancelPolicyHandlerTests
{
    [Fact]
    public async Task Handle_WhenPolicyDoesNotExist_ReturnsNotFoundWithoutSaving()
    {
        var repository = new StubPolicyRepository();

        var handler = new CancelPolicyHandler(
            repository,
            TimeProvider.System);

        var command = new CancelPolicyCommand(
            Reference: "HH-2026-999999",
            RefundReference: "REFUND-000001");

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
    public async Task Handle_WhenCancellationIsValid_CancelsAndSavesPolicy()
    {
        var policy = PolicyTestData.CreateValid();
        var repository = new StubPolicyRepository(
            policy: policy);

        var timeProvider = new FixedTimeProvider(
            new DateTimeOffset(2026, 2, 10, 0, 0, 0,
                TimeSpan.Zero));

        var handler = new CancelPolicyHandler(
            repository,
            timeProvider);

        var command = new CancelPolicyCommand(
            Reference: policy.Reference,
            RefundReference: "REFUND-000001");

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
            PolicyStatus.Cancelled,
            result.Value.Status);

        Assert.Equal(
            new DateOnly(2026, 2, 10),
            result.Value.CancellationDate);

        Assert.Equal(
            "REFUND-000001",
            result.Value.Refund?.Reference);
    }

    [Fact]
    public async Task Handle_WhenDomainCancellationFails_ReturnsFailureWithoutSaving()
    {
        var policy = PolicyTestData.CreateValid();

        var initialCancellationResult = policy.Cancel(
            policy.StartDate.AddDays(-1),
            "REFUND-000001");

        Assert.True(initialCancellationResult.IsSuccess);

        var repository = new StubPolicyRepository(
            policy: policy);

        var timeProvider = new FixedTimeProvider(
            new DateTimeOffset(2026, 2, 10, 0, 0, 0,
                TimeSpan.Zero));

        var handler = new CancelPolicyHandler(
            repository,
            timeProvider);

        var command = new CancelPolicyCommand(
            Reference: policy.Reference,
            RefundReference: "REFUND-000002");

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.False(
            repository.SaveChangesWasCalled);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "policy.already_cancelled",
            error.Code);
    }
}