using PolicyService.Application.Abstractions.Persistence;
using PolicyService.Domain.Policies;

namespace PolicyService.Application.Tests.Common;

internal sealed class StubPolicyRepository : IPolicyRepository
{
    private readonly Policy? _policy;
    private readonly bool _referenceExists;

    internal StubPolicyRepository(
        Policy? policy = null,
        bool referenceExists = false)
    {
        _policy = policy;
        _referenceExists = referenceExists;
    }

    internal Policy? AddedPolicy { get; private set; }

    internal bool AddWasCalled => AddedPolicy is not null;

    public Task<Policy?> GetByReferenceAsync(
        string reference,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_policy);
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