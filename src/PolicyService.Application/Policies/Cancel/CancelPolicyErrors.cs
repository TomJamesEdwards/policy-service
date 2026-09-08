using PolicyService.Domain.Common;

namespace PolicyService.Application.Policies.Cancel;

public static class CancelPolicyErrors
{
    public static DomainError NotFound(string reference) => new(
            Code: "policy.not_found",
            Message: $"Policy with reference '{reference}' was not found.");
}