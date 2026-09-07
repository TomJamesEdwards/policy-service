using PolicyService.Domain.Common;

namespace PolicyService.Domain.Policies;

internal static class PaymentErrors
{
    internal static readonly DomainError AmountMustBePositive = new(
        Code: "payment.amount.must_be_positive",
        Message: "Payment amount must be greater than zero.");

    internal static readonly DomainError ReferenceRequired = new(
        Code: "payment.reference.required",
        Message: "Payment reference is required.");

    internal static readonly DomainError InvalidCardNumber = new(
        Code: "payment.card_number.invalid",
        Message: "Card number is invalid.");
}