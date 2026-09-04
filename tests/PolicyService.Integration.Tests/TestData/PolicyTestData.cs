using PolicyService.Domain.Policies;

namespace PolicyService.IntegrationTests.TestData;

internal static class PolicyTestData
{
    internal static Policy CreateValid(
        string reference = "HH-2026-000001")
    {
        var property = InsuredProperty.Create(
            addressLine1: "1 Test Street",
            addressLine2: null,
            addressLine3: null,
            postcode: "CH7 1AA").Value;

        return Policy.Sell(
            reference,
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