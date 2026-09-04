using PolicyService.Application.Abstractions.Persistence;
using PolicyService.Application.Policies.Sell;
using PolicyService.Domain.Policies;
using Xunit;

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

    private sealed class StubPolicyRepository : IPolicyRepository
    {
        private readonly bool _referenceExists;

        internal StubPolicyRepository(bool referenceExists)
        {
            _referenceExists = referenceExists;
        }

        internal Policy? AddedPolicy { get; private set; }

        internal bool AddWasCalled => AddedPolicy is not null;

        public Task<Policy?> GetByReferenceAsync(
            string reference,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<Policy?>(null);
        }

        public Task<bool> ReferenceExistsAsync(
            string reference,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_referenceExists);
        }

        public Task AddAsync(
            Policy policy,
            CancellationToken cancellationToken)
        {
            AddedPolicy = policy;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        internal FixedTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
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