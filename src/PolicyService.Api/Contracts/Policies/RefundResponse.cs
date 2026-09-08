using PolicyService.Domain.Policies;

namespace PolicyService.Api.Contracts.Policies;

public sealed record RefundResponse(
    string Reference,
    PaymentType Type,
    decimal Amount);