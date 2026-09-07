using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PolicyService.Domain.Policies;
using PolicyService.Infrastructure.Persistence;
using PolicyService.IntegrationTests.TestData;
using Xunit;

namespace PolicyService.IntegrationTests.Persistence;

public sealed class PolicyDbContextTests
{
    [Fact]
    public async Task MigrateAndReload_WithValidPolicy_PreservesAggregate()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<PolicyDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new PolicyDbContext(options);

        await context.Database.MigrateAsync();

        var policy = PolicyTestData.CreateValid();

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

    [Fact]
    public async Task MigrateAndReload_WithCancelledPolicy_PreservesCancellation()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<PolicyDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new PolicyDbContext(options);

        await context.Database.MigrateAsync();

        var policy = PolicyTestData.CreateValid();
        var cancellationDate = policy.StartDate.AddDays(-1);

        var cancellationResult = policy.Cancel(
            cancellationDate,
            "REFUND-000001");

        Assert.True(cancellationResult.IsSuccess);

        context.Policies.Add(policy);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var reloadedPolicy = await context.Policies
            .SingleAsync(candidate =>
                candidate.Reference == policy.Reference);

        Assert.Equal(
            PolicyStatus.Cancelled,
            reloadedPolicy.Status);

        Assert.Equal(
            cancellationDate,
            reloadedPolicy.CancellationDate);

        var refund = Assert.IsType<Refund>(
            reloadedPolicy.Refund);

        Assert.Equal(
            "REFUND-000001",
            refund.Reference);

        Assert.Equal(
            policy.OriginalPayment.Type,
            refund.Type);

        Assert.Equal(
            policy.Amount,
            refund.Amount);
    }

    [Fact]
    public async Task MigrateAndReload_WithRenewedPolicy_PreservesRenewal()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<PolicyDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new PolicyDbContext(options);

        await context.Database.MigrateAsync();

        var policy = PolicyTestData.CreateValid();

        context.Policies.Add(policy);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var trackedPolicy = await context.Policies
            .SingleAsync(candidate =>
                candidate.Reference == policy.Reference);

        var originalEndDate = trackedPolicy.EndDate;

        var renewalResult = trackedPolicy.Renew(
            originalEndDate.AddDays(-30),
            paymentReference: "PAY-000002");

        Assert.True(renewalResult.IsSuccess);

        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();

        var reloadedPolicy = await context.Policies
            .SingleAsync(candidate =>
                candidate.Reference == policy.Reference);

        Assert.Equal(
            originalEndDate.AddYears(1),
            reloadedPolicy.EndDate);

        Assert.Equal(
            2,
            reloadedPolicy.Payments.Count);

        var renewalPayment =
            reloadedPolicy.Payments[^1];

        Assert.Equal(
            "PAY-000002",
            renewalPayment.Reference);

        Assert.Equal(
            PaymentType.DirectDebit,
            renewalPayment.Type);

        Assert.Equal(
            policy.Amount,
            renewalPayment.Amount);
    }

}