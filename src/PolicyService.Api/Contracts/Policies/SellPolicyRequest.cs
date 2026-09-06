using PolicyService.Domain.Policies;

namespace PolicyService.Api.Contracts.Policies;

public sealed record SellPolicyRequest(
    string Reference,
    PolicyType Type,
    DateOnly StartDate,
    decimal Amount,
    bool AutoRenew,
    IReadOnlyCollection<PolicyholderRequest> Policyholders,
    InsuredPropertyRequest Property,
    PaymentRequest Payment);