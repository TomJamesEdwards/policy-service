namespace PolicyService.Api.Contracts.Policies;

public sealed record CancelPolicyRequest(
    string? RefundReference);