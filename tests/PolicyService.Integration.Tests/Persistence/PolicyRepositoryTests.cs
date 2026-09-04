using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PolicyService.Infrastructure.Persistence;
using PolicyService.IntegrationTests.TestData;
using Xunit;

namespace PolicyService.IntegrationTests.Persistence;

public sealed class PolicyRepositoryTests
{
    [Fact]
    public async Task AddAndGetByReference_WithValidPolicy_ReturnsAggregate()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<PolicyDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new PolicyDbContext(options);

        await context.Database.EnsureCreatedAsync();

        var repository = new PolicyRepository(context);
        var policy = PolicyTestData.CreateValid();

        await repository.AddAsync(
            policy,
            CancellationToken.None);

        context.ChangeTracker.Clear();

        var retrievedPolicy = await repository.GetByReferenceAsync(
            policy.Reference,
            CancellationToken.None);

        Assert.NotNull(retrievedPolicy);
        Assert.Equal(policy.Reference, retrievedPolicy.Reference);
        Assert.Single(retrievedPolicy.Policyholders);
        Assert.Single(retrievedPolicy.Payments);
        Assert.Equal(
            policy.Property.Postcode,
            retrievedPolicy.Property.Postcode);
    }

    [Fact]
    public async Task ReferenceExists_BeforeAndAfterSaving_ReturnsExpectedResult()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<PolicyDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new PolicyDbContext(options);

        await context.Database.EnsureCreatedAsync();

        var repository = new PolicyRepository(context);
        var policy = PolicyTestData.CreateValid();

        var existsBeforeSaving =
            await repository.ReferenceExistsAsync(
                policy.Reference,
                CancellationToken.None);

        await repository.AddAsync(
            policy,
            CancellationToken.None);

        var existsAfterSaving =
            await repository.ReferenceExistsAsync(
                policy.Reference,
                CancellationToken.None);

        Assert.False(existsBeforeSaving);
        Assert.True(existsAfterSaving);
    }

}