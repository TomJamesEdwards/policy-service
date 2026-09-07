using PolicyService.Domain.Common;

namespace PolicyService.Application.Policies.Renew;

public static class RenewPolicyErrors
{
    public static DomainError NotFound(string reference) => new(
        Code: "policy.not_found",
        Message: $"Policy with reference '{reference}' was not found.");
}