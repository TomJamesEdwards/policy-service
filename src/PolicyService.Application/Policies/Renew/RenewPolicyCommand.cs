namespace PolicyService.Application.Policies.Renew;

public sealed record RenewPolicyCommand(
    string Reference,
    string? PaymentReference,
    string? CardNumber);