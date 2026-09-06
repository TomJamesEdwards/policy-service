using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PolicyService.Infrastructure.Persistence;
using PolicyService.Domain.Policies;

namespace PolicyService.IntegrationTests.Api;

internal sealed class PolicyServiceApiFactory
    : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection =
        new("Data Source=:memory:");

    internal PolicyServiceApiFactory()
    {
        _connection.Open();
    }

    internal async Task InitialiseDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<PolicyDbContext>();

        await context.Database.MigrateAsync();
    }

    internal async Task SeedPolicyAsync(Policy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        using var scope = Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<PolicyDbContext>();

        context.Policies.Add(policy);

        await context.SaveChangesAsync();
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<
                DbContextOptions<PolicyDbContext>>();

            services.RemoveAll<PolicyDbContext>();

            services.AddDbContext<PolicyDbContext>(options =>
                options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}