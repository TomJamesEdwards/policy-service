using PolicyService.Domain.Common;

namespace PolicyService.Domain.Policies;

public sealed class Policy
{
    private const int MinimumPolicyholderAge = 16;
    private const int CoolingOffPeriodDays = 14;

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

    public PolicyStatus Status { get; private set; } =
    PolicyStatus.Active;

    public DateOnly? CancellationDate { get; private set; }

    public Refund? Refund { get; private set; }

    public Payment OriginalPayment => Payments[0];

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

    public Result<CancellationQuote> CalculateCancellationQuote(
    DateOnly cancellationDate)
    {
        // A claim removes entitlement to a refund, regardless of
        // when the cancellation occurs.
        if (HasClaims)
        {
            return Result.Success(
                new CancellationQuote(0m));
        }

        // Cancellations before cover begins, or on policy days 0–13,
        // fall within the full-refund period.
        var fullRefundPeriodEndExclusive =
            StartDate.AddDays(CoolingOffPeriodDays);

        if (cancellationDate < fullRefundPeriodEndExclusive)
        {
            return Result.Success(
                new CancellationQuote(Amount));
        }

        // The policy covers both its start and end dates.
        var totalPolicyDays =
            EndDate.DayNumber - StartDate.DayNumber + 1;

        // The cancellation date is used cover cover; refundable unused cover
        // therefore begins on the following day.
        var unusedPolicyDays = Math.Max(
            EndDate.DayNumber - cancellationDate.DayNumber,
            0);

        // Monetary refunds are explicitly rounded to two decimal places.
        var refundAmount = decimal.Round(
            Amount * unusedPolicyDays / totalPolicyDays,
            2,
            MidpointRounding.AwayFromZero);

        return Result.Success(
            new CancellationQuote(refundAmount));
    }

    public Result<Policy> Cancel(
        DateOnly cancellationDate,
        string? refundReference)
    {
        if (Status == PolicyStatus.Cancelled)
        {
            return Result.Failure<Policy>(
                PolicyErrors.AlreadyCancelled);
        }

        var quoteResult =
            CalculateCancellationQuote(cancellationDate);

        if (quoteResult.IsFailure)
        {
            return Result.Failure<Policy>(
                quoteResult.Errors);
        }

        Refund? refund = null;

        if (quoteResult.Value.RefundAmount > 0m)
        {
            if (string.IsNullOrWhiteSpace(refundReference))
            {
                return Result.Failure<Policy>(
                    PolicyErrors.RefundReferenceRequired);
            }

            refund = new Refund(
                refundReference,
                OriginalPayment.Type,
                quoteResult.Value.RefundAmount);
        }

        Status = PolicyStatus.Cancelled;
        CancellationDate = cancellationDate;
        Refund = refund;

        return Result.Success(this);
    }

}