using PolicyService.Domain.Common;

namespace PolicyService.Domain.Policies;

internal static class PolicyErrors
{
    public static DomainError StartDateTooFarInAdvance { get; } = new(
        Code: "policy.start_date_too_far_in_advance",
        Message: "A policy cannot start more than 60 days in advance.");

    public static DomainError InvalidPolicyholderCount { get; } = new(
        Code: "policy.policyholders.invalid_count",
        Message: "A policy must have between 1 and 3 policyholders.");

    public static DomainError PolicyholderBelowMinimumAge { get; } = new(
        Code: "policy.policyholders.minimum_age",
        Message: "All policyholders must be at least 16 on the policy start date.");
}