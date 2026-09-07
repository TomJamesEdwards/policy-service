using PolicyService.Domain.Policies;

namespace PolicyService.Api.Contracts.Policies;

public sealed record PolicyResponse(
    string Reference,
    PolicyType Type,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Amount,
    bool AutoRenew,
    bool HasClaims,
    PolicyStatus Status,
    DateOnly? CancellationDate,
    RefundResponse? Refund,
    IReadOnlyCollection<PolicyholderResponse> Policyholders,
    InsuredPropertyResponse Property,
    IReadOnlyCollection<PaymentResponse> Payments)
{
    public static PolicyResponse FromDomain(Policy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        return new PolicyResponse(
            policy.Reference,
            policy.Type,
            policy.StartDate,
            policy.EndDate,
            policy.Amount,
            policy.AutoRenew,
            policy.HasClaims,
            policy.Status,
            policy.CancellationDate,
            policy.Refund is null
                ? null
                : new RefundResponse(
                    policy.Refund.Reference,
                    policy.Refund.Type,
                    policy.Refund.Amount),
            policy.Policyholders
                .Select(policyholder =>
                    new PolicyholderResponse(
                        policyholder.FirstName,
                        policyholder.LastName,
                        policyholder.DateOfBirth))
                .ToArray(),
            new InsuredPropertyResponse(
                policy.Property.AddressLine1,
                policy.Property.AddressLine2,
                policy.Property.AddressLine3,
                policy.Property.Postcode),
            policy.Payments
                .Select(payment =>
                    new PaymentResponse(
                        payment.Reference,
                        payment.Type,
                        payment.Amount))
                .ToArray());
    }
}