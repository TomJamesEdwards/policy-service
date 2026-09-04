using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PolicyService.Infrastructure.Persistence;

public sealed class PolicyDbContextFactory
    : IDesignTimeDbContextFactory<PolicyDbContext>
{
    private const string DefaultConnectionString =
        "Data Source=policy-service.db";

    public PolicyDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PolicyDbContext>()
            .UseSqlite(DefaultConnectionString)
            .Options;

        return new PolicyDbContext(options);
    }
}