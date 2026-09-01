using PolicyService.Domain.Policies;
using Xunit;

namespace PolicyService.Domain.Tests.Policies;

public sealed class PolicyTests
{

    [Fact]
    public void Sell_WithValidDetails_CreatesOneYearPolicyAndOriginalPayment()
    {
        var today = new DateOnly(2026, 9, 1);
        var startDate = new DateOnly(2026, 10, 1);

        var result = new PolicySaleBuilder()
            .WithToday(today)
            .WithStartDate(startDate)
            .Sell();
        Assert.True(result.IsSuccess);

        var policy = result.Value;

        Assert.Equal("HH-2026-000001", policy.Reference);
        Assert.Equal(startDate, policy.StartDate);
        Assert.Equal(new DateOnly(2027, 09, 30), policy.EndDate);
        Assert.Equal(350.50m, policy.Amount);
        Assert.Single(policy.Policyholders);

        var payment = Assert.Single(policy.Payments);
        Assert.Equal("PAY-000001", payment.Reference);
        Assert.Equal(PaymentType.DirectDebit, payment.Type);
        Assert.Equal(350.50m, payment.Amount);
    }

    [Fact]
    public void Sell_WhenStartDateIsMoreThanSixtyDaysAway_ReturnsValidationFailure()
    {
        var today = new DateOnly(2026, 9, 1);

        var result = new PolicySaleBuilder()
            .WithToday(today)
            .WithStartDate(today.AddDays(61))
            .Sell();

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "policy.start_date_too_far_in_advance",
            error.Code);

        Assert.Equal(
            "A policy cannot start more than 60 days in advance.",
            error.Message);
    }

    [Fact]
    public void Sell_WhenStartDateIsExactlySixtyDaysAway_Succeeds()
    {
        var today = new DateOnly(2026, 9, 1);

        var result = new PolicySaleBuilder()
            .WithToday(today)
            .WithStartDate(today.AddDays(60))
            .Sell();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Sell_WithNoPolicyholders_ReturnsValidationFailure()
    {
        var result = new PolicySaleBuilder()
            .WithPolicyholders()
            .Sell();

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "policy.policyholders.invalid_count",
            error.Code);

        Assert.Equal(
            "A policy must have between 1 and 3 policyholders.",
            error.Message);
    }

    [Fact]
    public void Sell_WithThreePolicyholders_Succeeds()
    {
        var result = new PolicySaleBuilder()
            .WithPolicyholderCount(3)
            .Sell();

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Policyholders.Count);
    }

    [Fact]
    public void Sell_WithFourPolicyholders_ReturnsValidationFailure()
    {
        var result = new PolicySaleBuilder()
            .WithPolicyholderCount(4)
            .Sell();

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "policy.policyholders.invalid_count",
            error.Code);

        Assert.Equal(
            "A policy must have between 1 and 3 policyholders.",
            error.Message);
    }

    [Fact]
    public void Sell_WhenPolicyholderIsUnderSixteenOnStartDate_ReturnsValidationFailure()
    {
        var startDate = new DateOnly(2026, 10, 1);

        var underagePolicyholder = new Policyholder(
            firstName: "Alex",
            lastName: "Example",
            dateOfBirth: new DateOnly(2010, 10, 2));

        var result = new PolicySaleBuilder()
            .WithStartDate(startDate)
            .WithPolicyholders(underagePolicyholder)
            .Sell();

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "policy.policyholders.minimum_age",
            error.Code);

        Assert.Equal(
            "All policyholders must be at least 16 on the policy start date.",
            error.Message);
    }

    [Fact]
    public void Sell_WhenPolicyholderTurnsSixteenOnStartDate_Succeeds()
    {
        var startDate = new DateOnly(2026, 10, 1);

        var policyholder = new Policyholder(
            firstName: "Anne",
            lastName: "Example",
            dateOfBirth: new DateOnly(2010, 10, 1));

        var result = new PolicySaleBuilder()
            .WithStartDate(startDate)
            .WithPolicyholders(policyholder)
            .Sell();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Sell_WhenAnyPolicyholderIsUnderSixteen_ReturnsValidationFailure()
    {
        var startDate = new DateOnly(2026, 10, 1);

        var eligiblePolicyholder = new Policyholder(
            firstName: "Anne",
            lastName: "Example",
            dateOfBirth: new DateOnly(1990, 4, 12));

        var underagePolicyholder = new Policyholder(
            firstName: "Bob",
            lastName: "Example",
            dateOfBirth: new DateOnly(2010, 10, 2));

        var result = new PolicySaleBuilder()
            .WithStartDate(startDate)
            .WithPolicyholders(
                eligiblePolicyholder,
                underagePolicyholder)
            .Sell();

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "policy.policyholders.minimum_age",
            error.Code);
    }

}
