using PolicyService.Domain.Common;

namespace PolicyService.Domain.Policies;

public sealed class Policy
{
    private Policy(
        string reference,
        PolicyType type,
        DateOnly startDate,
        DateOnly endDate,
        decimal amount,
        bool autoRenew,
        bool hasClaims,
        IReadOnlyCollection<Policyholder> policyholders,
        InsuredProperty property,
        Payment payment)
    {
        Reference = reference;
        Type = type;
        StartDate = startDate;
        EndDate = endDate;
        Amount = amount;
        AutoRenew = autoRenew;
        HasClaims = hasClaims;
        Policyholders = policyholders.ToList().AsReadOnly();
        Property = property;
        Payments = new List<Payment> { payment }.AsReadOnly();
    }

    public string Reference { get; }

    public PolicyType Type { get; }

    public DateOnly StartDate { get; }

    public DateOnly EndDate { get; }

    public decimal Amount { get; }

    public bool AutoRenew { get; }

    public bool HasClaims { get; }

    public IReadOnlyList<Policyholder> Policyholders { get; }

    public InsuredProperty Property { get; }

    public IReadOnlyList<Payment> Payments { get; }

    public static Result<Policy> Sell(
        string reference,
        PolicyType type,
        DateOnly startDate,
        decimal amount,
        bool autoRenew,
        bool hasClaims,
        IReadOnlyCollection<Policyholder> policyholders,
        InsuredProperty property,
        string paymentReference,
        PaymentType paymentType,
        DateOnly today)
    {
        if (startDate > today.AddDays(60))
        {
            return Result.Failure<Policy>(
                PolicyErrors.StartDateTooFarInAdvance);
        }
        var endDate = startDate.AddYears(1).AddDays(-1);

        var payment = new Payment(
            paymentReference,
            paymentType,
            amount);

        var policy = new Policy(
            reference,
            type,
            startDate,
            endDate,
            amount,
            autoRenew,
            hasClaims,
            policyholders,
            property,
            payment);

        return Result.Success(policy);
    }
}