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

        var policyholder = new Policyholder(
            firstName: "Anne",
            lastName: "Example",
            dateOfBirth: new DateOnly(1990, 4, 12)
        );

        var property = new InsuredProperty(
            addressLine1: "1 Example Street",
            addressLine2: null,
            addressLine3: null,
            postcode: "CH7 1AA"
        );

        var result = Policy.Sell(
            reference: "HH-2026-000001",
            type: PolicyType.Household,
            startDate: startDate,
            amount: 350.50m,
            autoRenew: true,
            hasClaims: false,
            policyholders: [policyholder],
            property: property,
            paymentReference: "PAY-000001",
            paymentType: PaymentType.DirectDebit,
            today: today
        );

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
        var startDate = today.AddDays(61);

        var policyholder = new Policyholder(
            firstName: "Anne",
            lastName: "Example",
            dateOfBirth: new DateOnly(1990, 4, 12));

        var property = new InsuredProperty(
            addressLine1: "1 Example Street",
            addressLine2: null,
            addressLine3: null,
            postcode: "CH7 1AA");

        var result = Policy.Sell(
            reference: "HH-2026-000002",
            type: PolicyType.Household,
            startDate: startDate,
            amount: 420.50m,
            autoRenew: true,
            hasClaims: false,
            policyholders: [policyholder],
            property: property,
            paymentReference: "PAY-000002",
            paymentType: PaymentType.DirectDebit,
            today: today);

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
        var startDate = today.AddDays(60);

        var policyholder = new Policyholder(
            firstName: "Anne",
            lastName: "Example",
            dateOfBirth: new DateOnly(1990, 4, 12));

        var property = new InsuredProperty(
            addressLine1: "1 Example Street",
            addressLine2: null,
            addressLine3: null,
            postcode: "CH7 1AA");

        var result = Policy.Sell(
            reference: "HH-2026-000003",
            type: PolicyType.Household,
            startDate: startDate,
            amount: 420.50m,
            autoRenew: true,
            hasClaims: false,
            policyholders: [policyholder],
            property: property,
            paymentReference: "PAY-000003",
            paymentType: PaymentType.DirectDebit,
            today: today);

        Assert.True(result.IsSuccess);
    }

}
