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
}
