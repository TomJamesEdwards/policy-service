using PolicyService.Domain.Common;

namespace PolicyService.Domain.Policies;

public sealed class Policy
{
    private const int MinimumPolicyholderAge = 16;

    private readonly List<Policyholder> _policyholders = [];
    private readonly List<Payment> _payments = [];

    private Policy()
    {
    }

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
        Property = property;

        _policyholders.AddRange(policyholders);
        _payments.Add(payment);
    }

    public string Reference { get; private set; } = null!;

    public PolicyType Type { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public decimal Amount { get; private set; }

    public bool AutoRenew { get; private set; }

    public bool HasClaims { get; private set; }

    public IReadOnlyList<Policyholder> Policyholders =>
        _policyholders.AsReadOnly();

    public InsuredProperty Property { get; private set; } = null!;

    public IReadOnlyList<Payment> Payments =>
        _payments.AsReadOnly();

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
        DateOnly today,
        string? cardNumber = null)
    {
        if (startDate > today.AddDays(60))
        {
            return Result.Failure<Policy>(
                PolicyErrors.StartDateTooFarInAdvance);
        }
        if (policyholders.Count < 1 || policyholders.Count > 3)
        {
            return Result.Failure<Policy>(
                PolicyErrors.InvalidPolicyholderCount);
        }
        var latestEligibleDateOfBirth =
            startDate.AddYears(-MinimumPolicyholderAge);

        if (policyholders.Any(policyholder =>
                policyholder.DateOfBirth > latestEligibleDateOfBirth))
        {
            return Result.Failure<Policy>(
                PolicyErrors.PolicyholderBelowMinimumAge);
        }
        var endDate = startDate.AddYears(1).AddDays(-1);

        var paymentResult = Payment.Create(
            paymentReference,
            paymentType,
            amount,
            cardNumber);

        if (paymentResult.IsFailure)
        {
            return Result.Failure<Policy>(
                paymentResult.Errors);
        }

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
            paymentResult.Value);

        return Result.Success(policy);
    }
}