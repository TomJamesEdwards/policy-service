using PolicyService.Domain.Policies;

namespace PolicyService.Application.Abstractions.Persistence;

public interface IPolicyRepository
{
    Task<Policy?> GetByReferenceAsync(
        string reference,
        CancellationToken cancellationToken);

    Task<bool> ReferenceExistsAsync(
        string reference,
        CancellationToken cancellationToken);

    Task AddAsync(
        Policy policy,
        CancellationToken cancellationToken);
}