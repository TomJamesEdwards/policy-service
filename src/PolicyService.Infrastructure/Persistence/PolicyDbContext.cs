using Microsoft.EntityFrameworkCore;
using PolicyService.Domain.Policies;
using PolicyService.Infrastructure.Persistence.Configurations;

namespace PolicyService.Infrastructure.Persistence;

public sealed class PolicyDbContext : DbContext
{
    public PolicyDbContext(
        DbContextOptions<PolicyDbContext> options)
        : base(options)
    {
    }

    public DbSet<Policy> Policies => Set<Policy>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(
            new PolicyConfiguration());
    }
}