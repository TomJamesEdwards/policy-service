using PolicyService.Domain.Common;

namespace PolicyService.Domain.Policies;

internal static class PaymentErrors
{
    internal static readonly DomainError AmountMustBePositive = new(
        "payment.amount.must_be_positive",
        "Payment amount must be greater than zero.");
}