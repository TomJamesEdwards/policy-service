using PolicyService.Application.Abstractions.Persistence;
using PolicyService.Domain.Common;
using PolicyService.Domain.Policies;

namespace PolicyService.Application.Policies.Cancel;

public sealed class CancelPolicyHandler
{
    private readonly IPolicyRepository _repository;
    private readonly TimeProvider _timeProvider;

    public CancelPolicyHandler(
        IPolicyRepository repository,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _repository = repository;
        _timeProvider = timeProvider;
    }

    public async Task<Result<Policy>> Handle(
        CancelPolicyCommand command,
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
                CancelPolicyErrors.NotFound(
                    command.Reference));
        }

        var cancellationDate = DateOnly.FromDateTime(
            _timeProvider.GetUtcNow().UtcDateTime);

        var cancellationResult = policy.Cancel(
            cancellationDate,
            command.RefundReference);

        if (cancellationResult.IsFailure)
        {
            return cancellationResult;
        }

        await _repository
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);

        return cancellationResult;
    }
}