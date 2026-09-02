using PolicyService.Domain.Common;

namespace PolicyService.Domain.Policies;

public sealed class Payment
{
    private Payment(
        string reference,
        PaymentType type,
        decimal amount)
    {
        Reference = reference;
        Type = type;
        Amount = amount;
    }

    public string Reference { get; }

    public PaymentType Type { get; }

    public decimal Amount { get; }

    public static Result<Payment> Create(
        string reference,
        PaymentType type,
        decimal amount)
    {
        if (amount <= 0m)
        {
            return Result.Failure<Payment>(
                PaymentErrors.AmountMustBePositive);
        }

        return Result.Success(
            new Payment(reference, type, amount));
    }
}