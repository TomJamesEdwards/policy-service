using PolicyService.Domain.Common;

namespace PolicyService.Application.Policies.Sell;

internal static class SellPolicyErrors
{
    internal static DomainError ReferenceConflict(string reference)
    {
        return new DomainError(
            "policy.reference.conflict",
            $"A policy with reference '{reference}' already exists.");
    }
}