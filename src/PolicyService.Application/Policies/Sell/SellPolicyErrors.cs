using PolicyService.Domain.Common;

namespace PolicyService.Application.Policies.Sell;

internal static class SellPolicyErrors
{
    internal static DomainError ReferenceConflict(string reference) => new(
            Code: "policy.reference.conflict",
            Message: $"A policy with reference '{reference}' already exists.");
}