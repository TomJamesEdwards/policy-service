using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PolicyService.Application.Abstractions.Persistence;
using PolicyService.Infrastructure.Persistence;

namespace PolicyService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<PolicyDbContext>(options =>
            options.UseSqlite(connectionString));

        services
            .AddHealthChecks()
            .AddDbContextCheck<PolicyDbContext>(
                name: "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready"]);

        services.AddScoped<IPolicyRepository, PolicyRepository>();

        return services;
    }
}