using PolicyService.Application.Abstractions.Persistence;
using PolicyService.Domain.Common;
using PolicyService.Domain.Policies;

namespace PolicyService.Application.Policies.GetByReference;

public sealed class GetPolicyByReferenceHandler
{
    private readonly IPolicyRepository _repository;

    public GetPolicyByReferenceHandler(IPolicyRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Policy>> Handle(
        GetPolicyByReferenceQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var policy = await _repository
            .GetByReferenceAsync(query.Reference, cancellationToken)
            .ConfigureAwait(false);

        if (policy is null)
        {
            return Result.Failure<Policy>(
                PolicyLookupErrors.NotFound(query.Reference));
        }

        return Result.Success(policy);
    }
}