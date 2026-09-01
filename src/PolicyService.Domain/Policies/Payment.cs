namespace PolicyService.Domain.Policies;

public sealed class Payment
{
    internal Payment(
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
}