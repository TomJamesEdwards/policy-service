namespace PolicyService.Domain.Policies;

public sealed class Refund
{
    private Refund()
    {
    }

    internal Refund(
        string reference,
        PaymentType type,
        decimal amount)
    {
        Reference = reference;
        Type = type;
        Amount = amount;
    }

    public string Reference { get; private set; } =
        string.Empty;

    public PaymentType Type { get; private set; }

    public decimal Amount { get; private set; }
}