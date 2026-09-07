using PolicyService.Application.Abstractions.Persistence;
using PolicyService.Domain.Common;
using PolicyService.Domain.Policies;

namespace PolicyService.Application.Policies.Renew;

public sealed class RenewPolicyHandler
{
    private readonly IPolicyRepository _repository;
    private readonly TimeProvider _timeProvider;

    public RenewPolicyHandler(
        IPolicyRepository repository,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<Result<Policy>> Handle(
        RenewPolicyCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var policy = await _repository
            .GetByReferenceForUpdateAsync(
                command.Reference,
                cancellationToken)
            .ConfigureAwait(false);

        if (policy is null)
        {
            return Result.Failure<Policy>(
                RenewPolicyErrors.NotFound(
                    command.Reference));
        }

        var renewalDate = DateOnly.FromDateTime(
            _timeProvider.GetUtcNow().UtcDateTime);

        var renewalResult = policy.Renew(
            renewalDate,
            command.PaymentReference,
            command.CardNumber);

        if (renewalResult.IsFailure)
        {
            return renewalResult;
        }

        await _repository
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return renewalResult;
    }
}