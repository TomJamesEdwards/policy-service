using PolicyService.Application.Policies.Sell;
using PolicyService.Domain.Policies;
using PolicyService.Application.Tests.Common;

namespace PolicyService.Application.Tests.Policies.Sell;

public sealed class SellPolicyHandlerTests
{

    private static SellPolicyCommand CreateCommand()
    {
        return new SellPolicyCommand(
            Reference: "HH-2026-000001",
            Type: PolicyType.Household,
            StartDate: new DateOnly(2026, 2, 1),
            Amount: 350.50m,
            AutoRenew: true,
            Policyholders:
            [
                new PolicyholderInput(
                    "Anne",
                    "Example",
                    new DateOnly(1990, 4, 12))
            ],
            Property: new InsuredPropertyInput(
                "1 Test Street",
                null,
                null,
                "CH7 1AA"),
            PaymentReference: "PAY-000001",
            PaymentType: PaymentType.DirectDebit,
            CardNumber: null);
    }

    [Fact]
    public async Task Handle_WhenReferenceAlreadyExists_ReturnsConflictWithoutSaving()
    {
        var repository = new StubPolicyRepository(referenceExists: true);
        var handler = new SellPolicyHandler(
            repository,
            TimeProvider.System);

        var command = CreateCommand();

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.False(repository.AddWasCalled);

        var error = Assert.Single(result.Errors);

        Assert.Equal("policy.reference.conflict", error.Code);
        Assert.Equal(
            "A policy with reference 'HH-2026-000001' already exists.",
            error.Message);
    }

    [Fact]
    public async Task Handle_WhenCommandIsValid_SavesAndReturnsPolicy()
    {
        var repository = new StubPolicyRepository(referenceExists: false);
        var timeProvider = new FixedTimeProvider(
            new DateTimeOffset(
                2026,
                1,
                1,
                0,
                0,
                0,
                TimeSpan.Zero));

        var handler = new SellPolicyHandler(
            repository,
            timeProvider);

        var command = CreateCommand();

        var result = await handler.Handle(
            command,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(repository.AddWasCalled);
        Assert.Same(result.Value, repository.AddedPolicy);
    }

    [Fact]
    public async Task Handle_WhenPropertyIsInvalid_ReturnsFailureWithoutSaving()
    {
        var repository = new StubPolicyRepository(referenceExists: false);
        var timeProvider = new FixedTimeProvider(
            new DateTimeOffset(
                2026,
                1,
                1,
                0,
                0,
                0,
                TimeSpan.Zero));

        var handler = new SellPolicyHandler(
            repository,
            timeProvider);

        var validCommand = CreateCommand();

        var invalidCommand = validCommand with
        {
            Property = validCommand.Property with
            {
                AddressLine1 = " "
            }
        };

        var result = await handler.Handle(
            invalidCommand,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.False(repository.AddWasCalled);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "property.address_line_1.required",
            error.Code);
    }

    [Fact]
    public async Task Handle_WhenPolicyIsInvalid_ReturnsFailureWithoutSaving()
    {
        var repository = new StubPolicyRepository(referenceExists: false);
        var timeProvider = new FixedTimeProvider(
            new DateTimeOffset(
                2026,
                1,
                1,
                0,
                0,
                0,
                TimeSpan.Zero));

        var handler = new SellPolicyHandler(
            repository,
            timeProvider);

        var invalidCommand = CreateCommand() with
        {
            Amount = 0m
        };

        var result = await handler.Handle(
            invalidCommand,
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.False(repository.AddWasCalled);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "payment.amount.must_be_positive",
            error.Code);
    }

}