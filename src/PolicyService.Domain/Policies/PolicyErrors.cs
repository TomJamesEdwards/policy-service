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

    public static DomainError RefundReferenceRequired { get; } = new(
        Code: "policy.refund_reference.required",
        Message: "A refund reference is required.");

    public static DomainError AlreadyCancelled { get; } = new(
        Code: "policy.already_cancelled",
        Message: "The policy has already been cancelled.");

    public static DomainError RenewalTooEarly { get; } = new(
        Code: "policy.renewal.too_early",
        Message: "The policy can only be renewed within 30 days of its end date.");

    public static DomainError RenewalAfterExpiry { get; } = new(
        Code: "policy.renewal.after_expiry",
        Message: "An expired policy cannot be renewed.");

    public static DomainError CancelledPolicyCannotBeRenewed { get; } = new(
        Code: "policy.renewal.cancelled",
        Message: "A cancelled policy cannot be renewed.");

    public static DomainError ChequeNotSupportedForAutoRenewal { get; } = new(
        Code: "policy.renewal.cheque_not_supported",
        Message: "Cheque payments cannot be used for automatic renewal.");
}