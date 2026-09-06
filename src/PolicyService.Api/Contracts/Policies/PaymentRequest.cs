using PolicyService.Domain.Policies;

namespace PolicyService.Api.Contracts.Policies;

public sealed record PaymentRequest(
    string Reference,
    PaymentType Type,
    string? CardNumber);