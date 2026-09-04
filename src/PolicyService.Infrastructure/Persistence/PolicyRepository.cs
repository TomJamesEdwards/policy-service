using Microsoft.EntityFrameworkCore;
using PolicyService.Application.Abstractions.Persistence;
using PolicyService.Domain.Policies;

namespace PolicyService.Infrastructure.Persistence;

public sealed class PolicyRepository : IPolicyRepository
{
    private readonly PolicyDbContext _context;

    public PolicyRepository(PolicyDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public Task<Policy?> GetByReferenceAsync(
        string reference,
        CancellationToken cancellationToken)
    {
        return _context.Policies
            .AsNoTracking()
            .SingleOrDefaultAsync(
                policy => policy.Reference == reference,
                cancellationToken);
    }

    public Task<bool> ReferenceExistsAsync(
        string reference,
        CancellationToken cancellationToken)
    {
        return _context.Policies.AnyAsync(
            policy => policy.Reference == reference,
            cancellationToken);
    }

    public async Task AddAsync(
        Policy policy,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(policy);

        _context.Policies.Add(policy);

        await _context
            .SaveChangesAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}