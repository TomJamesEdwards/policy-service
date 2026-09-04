using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PolicyService.Domain.Policies;
using PolicyService.Infrastructure.Persistence;
using Xunit;

namespace PolicyService.IntegrationTests.Persistence;

public sealed class PolicyDbContextTests
{
    [Fact]
    public async Task SaveAndReload_WithValidPolicy_PreservesAggregate()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<PolicyDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new PolicyDbContext(options);

        await context.Database.EnsureCreatedAsync();

        var policy = CreatePolicy();

        context.Policies.Add(policy);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var reloadedPolicy = await context.Policies
            .SingleAsync(candidate =>
                candidate.Reference == policy.Reference);

        Assert.Equal(policy.Reference, reloadedPolicy.Reference);
        Assert.Equal(policy.Type, reloadedPolicy.Type);
        Assert.Equal(policy.StartDate, reloadedPolicy.StartDate);
        Assert.Equal(policy.EndDate, reloadedPolicy.EndDate);
        Assert.Equal(policy.Amount, reloadedPolicy.Amount);
        Assert.Equal(policy.AutoRenew, reloadedPolicy.AutoRenew);

        var policyholder = Assert.Single(
            reloadedPolicy.Policyholders);

        Assert.Equal("Anne", policyholder.FirstName);
        Assert.Equal("Example", policyholder.LastName);

        Assert.Equal(
            "CH7 1AA",
            reloadedPolicy.Property.Postcode);

        var payment = Assert.Single(reloadedPolicy.Payments);

        Assert.Equal("PAY-000001", payment.Reference);
        Assert.Equal(PaymentType.DirectDebit, payment.Type);
        Assert.Equal(350.50m, payment.Amount);
    }

    private static Policy CreatePolicy()
    {
        var property = InsuredProperty.Create(
            addressLine1: "1 Test Street",
            addressLine2: null,
            addressLine3: null,
            postcode: "CH7 1AA").Value;

        return Policy.Sell(
            reference: "HH-2026-000001",
            type: PolicyType.Household,
            startDate: new DateOnly(2026, 2, 1),
            amount: 350.50m,
            autoRenew: true,
            hasClaims: false,
            policyholders:
            [
                new Policyholder(
                    "Anne",
                    "Example",
                    new DateOnly(1990, 1, 1))
            ],
            property,
            paymentReference: "PAY-000001",
            paymentType: PaymentType.DirectDebit,
            today: new DateOnly(2026, 1, 1)).Value;
    }
}