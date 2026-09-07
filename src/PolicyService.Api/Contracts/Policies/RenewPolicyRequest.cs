namespace PolicyService.Api.Contracts.Policies;

public sealed record RenewPolicyRequest(
    string? PaymentReference,
    string? CardNumber);