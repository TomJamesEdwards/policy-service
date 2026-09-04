using PolicyService.Application.Abstractions.Persistence;
using PolicyService.Domain.Common;
using PolicyService.Domain.Policies;

namespace PolicyService.Application.Policies.Sell;

public sealed class SellPolicyHandler
{
    private readonly IPolicyRepository _repository;
    private readonly TimeProvider _timeProvider;

    public SellPolicyHandler(
        IPolicyRepository repository,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<Result<Policy>> Handle(
        SellPolicyCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var referenceExists = await _repository
            .ReferenceExistsAsync(
                command.Reference,
                cancellationToken)
            .ConfigureAwait(false);

        if (referenceExists)
        {
            return Result.Failure<Policy>(
                SellPolicyErrors.ReferenceConflict(
                    command.Reference));
        }

        var propertyResult = InsuredProperty.Create(
            command.Property.AddressLine1,
            command.Property.AddressLine2,
            command.Property.AddressLine3,
            command.Property.Postcode);

        if (propertyResult.IsFailure)
        {
            return Result.Failure<Policy>(
                propertyResult.Errors);
        }

        var policyholders = command.Policyholders
            .Select(policyholder => new Policyholder(
                policyholder.FirstName,
                policyholder.LastName,
                policyholder.DateOfBirth))
            .ToArray();

        var today = DateOnly.FromDateTime(
            _timeProvider.GetUtcNow().UtcDateTime);

        var policyResult = Policy.Sell(
            reference: command.Reference,
            type: command.Type,
            startDate: command.StartDate,
            amount: command.Amount,
            autoRenew: command.AutoRenew,
            hasClaims: false,
            policyholders,
            property: propertyResult.Value,
            paymentReference: command.PaymentReference,
            paymentType: command.PaymentType,
            today,
            cardNumber: command.CardNumber);

        if (policyResult.IsFailure)
        {
            return policyResult;
        }

        await _repository
            .AddAsync(
                policyResult.Value,
                cancellationToken)
            .ConfigureAwait(false);

        return policyResult;
    }
}