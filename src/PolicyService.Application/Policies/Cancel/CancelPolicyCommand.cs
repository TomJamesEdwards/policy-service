namespace PolicyService.Application.Policies.Cancel;

public sealed record CancelPolicyCommand(
    string Reference,
    string? RefundReference);