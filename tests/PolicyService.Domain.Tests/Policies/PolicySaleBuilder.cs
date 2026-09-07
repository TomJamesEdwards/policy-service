using PolicyService.Domain.Common;
using PolicyService.Domain.Policies;

namespace PolicyService.Domain.Tests.Policies;

internal sealed class PolicySaleBuilder
{
    private DateOnly _today = new(2026, 9, 1);
    private DateOnly _startDate = new(2026, 10, 1);
    private bool _hasClaims;
    private bool _autoRenew = true;

    private IReadOnlyCollection<Policyholder> _policyholders =
    [
        new Policyholder(
            firstName: "Anne",
            lastName: "Example",
            dateOfBirth: new DateOnly(1990, 4, 12))
    ];

    private readonly InsuredProperty _insuredProperty =
         InsuredProperty.Create(
             addressLine1: "1 Test Street",
             addressLine2: null,
             addressLine3: null,
             postcode: "CH7 1AA").Value;


    private decimal _amount = 350.50m;

    private PaymentType _paymentType = PaymentType.DirectDebit;

    private string? _cardNumber;

    public PolicySaleBuilder WithToday(DateOnly today)
    {
        _today = today;
        return this;
    }

    public PolicySaleBuilder WithStartDate(DateOnly startDate)
    {
        _startDate = startDate;
        return this;
    }

    public PolicySaleBuilder WithPolicyholders(
        params Policyholder[] policyholders)
    {
        ArgumentNullException.ThrowIfNull(policyholders);

        _policyholders = policyholders.ToArray();
        return this;
    }

    public PolicySaleBuilder WithPolicyholderCount(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        _policyholders = Enumerable
            .Range(1, count)
            .Select(index => new Policyholder(
                firstName: $"Policyholder{index}",
                lastName: "Example",
                dateOfBirth: new DateOnly(1990, 4, 12)))
            .ToArray();

        return this;
    }

    public PolicySaleBuilder WithAmount(decimal amount)
    {
        _amount = amount;
        return this;
    }

    public PolicySaleBuilder WithCardPayment(string cardNumber)
    {
        _paymentType = PaymentType.Card;
        _cardNumber = cardNumber;

        return this;
    }

    public PolicySaleBuilder WithPaymentType(
    PaymentType paymentType)
    {
        _paymentType = paymentType;

        return this;
    }

    public PolicySaleBuilder WithClaims()
    {
        _hasClaims = true;

        return this;
    }

    public PolicySaleBuilder WithAutoRenew(
    bool autoRenew)
    {
        _autoRenew = autoRenew;
        return this;
    }

    public Result<Policy> Sell() =>
        Policy.Sell(
            reference: "HH-2026-000001",
            type: PolicyType.Household,
            startDate: _startDate,
            amount: _amount,
            autoRenew: _autoRenew,
            hasClaims: _hasClaims,
            policyholders: _policyholders,
            property: _insuredProperty,
            paymentReference: "PAY-000001",
            paymentType: _paymentType,
            today: _today,
            cardNumber: _cardNumber);
}