using PolicyService.Domain.Policies;

namespace PolicyService.Application.Policies.Sell;

public sealed record SellPolicyCommand(
    string Reference,
    PolicyType Type,
    DateOnly StartDate,
    decimal Amount,
    bool AutoRenew,
    IReadOnlyCollection<PolicyholderInput> Policyholders,
    InsuredPropertyInput Property,
    string PaymentReference,
    PaymentType PaymentType,
    string? CardNumber);