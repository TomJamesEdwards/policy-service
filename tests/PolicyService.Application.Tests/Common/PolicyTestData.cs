using PolicyService.Domain.Policies;

namespace PolicyService.Application.Tests.Common;

internal static class PolicyTestData
{
    internal static Policy CreateValid()
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
                    new DateOnly(1990, 4, 12))
            ],
            property,
            paymentReference: "PAY-000001",
            paymentType: PaymentType.DirectDebit,
            today: new DateOnly(2026, 1, 1)).Value;
    }
}