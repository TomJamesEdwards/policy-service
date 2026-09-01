using PolicyService.Domain.Common;

namespace PolicyService.Domain.Policies;

internal static class PolicyErrors
{
    public static DomainError StartDateTooFarInAdvance { get; } = new(
        Code: "policy.start_date_too_far_in_advance",
        Message: "A policy cannot start more than 60 days in advance.");
}