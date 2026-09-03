using PolicyService.Domain.Common;

namespace PolicyService.Application.Policies.GetByReference;

internal static class PolicyLookupErrors
{
    internal static DomainError NotFound(string reference)
    {
        return new DomainError(
            "policy.not_found",
            $"Policy '{reference}' was not found.");
    }
}