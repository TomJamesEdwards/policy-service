using PolicyService.Domain.Policies;

namespace PolicyService.Api.Contracts.Policies;

public sealed record PaymentResponse(
    string Reference,
    PaymentType Type,
    decimal Amount);