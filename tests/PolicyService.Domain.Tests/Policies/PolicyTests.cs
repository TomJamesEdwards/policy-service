using PolicyService.Domain.Policies;

namespace PolicyService.Domain.Tests.Policies;

public sealed class PolicyTests
{

    #region Policy Validation
    [Fact]
    public void Sell_WithValidDetails_CreatesOneYearPolicyAndOriginalPayment()
    {
        var today = new DateOnly(2026, 9, 1);
        var startDate = new DateOnly(2026, 10, 1);

        var result = new PolicySaleBuilder()
            .WithToday(today)
            .WithStartDate(startDate)
            .Sell();
        Assert.True(result.IsSuccess);

        var policy = result.Value;

        Assert.Equal("HH-2026-000001", policy.Reference);
        Assert.Equal(startDate, policy.StartDate);
        Assert.Equal(new DateOnly(2027, 09, 30), policy.EndDate);
        Assert.Equal(350.50m, policy.Amount);
        Assert.Single(policy.Policyholders);

        var payment = Assert.Single(policy.Payments);
        Assert.Equal("PAY-000001", payment.Reference);
        Assert.Equal(PaymentType.DirectDebit, payment.Type);
        Assert.Equal(350.50m, payment.Amount);
    }

    [Fact]
    public void Sell_WhenStartDateIsMoreThanSixtyDaysAway_ReturnsValidationFailure()
    {
        var today = new DateOnly(2026, 9, 1);

        var result = new PolicySaleBuilder()
            .WithToday(today)
            .WithStartDate(today.AddDays(61))
            .Sell();

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "policy.start_date_too_far_in_advance",
            error.Code);

        Assert.Equal(
            "A policy cannot start more than 60 days in advance.",
            error.Message);
    }

    [Fact]
    public void Sell_WhenStartDateIsExactlySixtyDaysAway_Succeeds()
    {
        var today = new DateOnly(2026, 9, 1);

        var result = new PolicySaleBuilder()
            .WithToday(today)
            .WithStartDate(today.AddDays(60))
            .Sell();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Sell_WithNoPolicyholders_ReturnsValidationFailure()
    {
        var result = new PolicySaleBuilder()
            .WithPolicyholders()
            .Sell();

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "policy.policyholders.invalid_count",
            error.Code);

        Assert.Equal(
            "A policy must have between 1 and 3 policyholders.",
            error.Message);
    }

    [Fact]
    public void Sell_WithThreePolicyholders_Succeeds()
    {
        var result = new PolicySaleBuilder()
            .WithPolicyholderCount(3)
            .Sell();

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Policyholders.Count);
    }

    [Fact]
    public void Sell_WithFourPolicyholders_ReturnsValidationFailure()
    {
        var result = new PolicySaleBuilder()
            .WithPolicyholderCount(4)
            .Sell();

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "policy.policyholders.invalid_count",
            error.Code);

        Assert.Equal(
            "A policy must have between 1 and 3 policyholders.",
            error.Message);
    }

    [Fact]
    public void Sell_WhenPolicyholderIsUnderSixteenOnStartDate_ReturnsValidationFailure()
    {
        var startDate = new DateOnly(2026, 10, 1);

        var underagePolicyholder = new Policyholder(
            firstName: "Alex",
            lastName: "Example",
            dateOfBirth: new DateOnly(2010, 10, 2));

        var result = new PolicySaleBuilder()
            .WithStartDate(startDate)
            .WithPolicyholders(underagePolicyholder)
            .Sell();

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "policy.policyholders.minimum_age",
            error.Code);

        Assert.Equal(
            "All policyholders must be at least 16 on the policy start date.",
            error.Message);
    }

    [Fact]
    public void Sell_WhenPolicyholderTurnsSixteenOnStartDate_Succeeds()
    {
        var startDate = new DateOnly(2026, 10, 1);

        var policyholder = new Policyholder(
            firstName: "Anne",
            lastName: "Example",
            dateOfBirth: new DateOnly(2010, 10, 1));

        var result = new PolicySaleBuilder()
            .WithStartDate(startDate)
            .WithPolicyholders(policyholder)
            .Sell();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Sell_WhenAnyPolicyholderIsUnderSixteen_ReturnsValidationFailure()
    {
        var startDate = new DateOnly(2026, 10, 1);

        var eligiblePolicyholder = new Policyholder(
            firstName: "Anne",
            lastName: "Example",
            dateOfBirth: new DateOnly(1990, 4, 12));

        var underagePolicyholder = new Policyholder(
            firstName: "Bob",
            lastName: "Example",
            dateOfBirth: new DateOnly(2010, 10, 2));

        var result = new PolicySaleBuilder()
            .WithStartDate(startDate)
            .WithPolicyholders(
                eligiblePolicyholder,
                underagePolicyholder)
            .Sell();

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "policy.policyholders.minimum_age",
            error.Code);
    }

    [Fact]
    public void Sell_WhenPaymentAmountIsZero_ReturnsValidationFailure()
    {
        var result = new PolicySaleBuilder()
            .WithAmount(0m)
            .Sell();

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "payment.amount.must_be_positive",
            error.Code);

        Assert.Equal(
            "Payment amount must be greater than zero.",
            error.Message);
    }

    #endregion
    #region Luhn tests
    [Fact]
    public void Sell_WithValidCardNumber_ReturnsSuccessfulPolicy()
    {
        var result = new PolicySaleBuilder()
            .WithCardPayment("4111111111111111")
            .Sell();

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Sell_WithInvalidCardNumber_ReturnsPaymentValidationFailure()
    {
        var result = new PolicySaleBuilder()
            .WithCardPayment("4111111111111112")
            .Sell();

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "payment.card_number.invalid",
            error.Code);
    }
    #endregion

    #region Cancellation
    [Fact]
    public void CalculateCancellationQuote_WhenBeforeStartDate_ReturnsFullRefund()
    {
        var startDate = new DateOnly(2026, 2, 1);

        var policyResult = new PolicySaleBuilder()
            .WithToday(new DateOnly(2026, 1, 1))
            .WithStartDate(startDate)
            .WithAmount(365m)
            .Sell();

        Assert.True(policyResult.IsSuccess);

        var result = policyResult.Value
            .CalculateCancellationQuote(
                startDate.AddDays(-1));

        Assert.True(result.IsSuccess);
        Assert.Equal(
            365m,
            result.Value.RefundAmount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public void CalculateCancellationQuote_WhenWithinCoolingOffPeriod_ReturnsFullRefund(
    int daysAfterStart)
    {
        var startDate = new DateOnly(2026, 2, 1);

        var policyResult = new PolicySaleBuilder()
            .WithToday(new DateOnly(2026, 1, 1))
            .WithStartDate(startDate)
            .WithAmount(365m)
            .Sell();

        Assert.True(policyResult.IsSuccess);

        var result = policyResult.Value
            .CalculateCancellationQuote(
                startDate.AddDays(daysAfterStart));

        Assert.True(result.IsSuccess);
        Assert.Equal(
            365m,
            result.Value.RefundAmount);
    }

    [Fact]
    public void CalculateCancellationQuote_OnFirstDayAfterCoolingOffPeriod_ReturnsProRataRefund()
    {
        var startDate = new DateOnly(2026, 2, 1);

        var policyResult = new PolicySaleBuilder()
            .WithToday(new DateOnly(2026, 1, 1))
            .WithStartDate(startDate)
            .WithAmount(365m)
            .Sell();

        Assert.True(policyResult.IsSuccess);

        var result = policyResult.Value
            .CalculateCancellationQuote((
                startDate.AddDays(14)));

        Assert.True(result.IsSuccess);
        Assert.Equal(
            350m,
            result.Value.RefundAmount);
    }

    [Fact]
    public void CalculateCancellationQuote_WhenRefundIsHalfPenny_RoundsAwayFromZero()
    {
        var startDate = new DateOnly(2027, 3, 1);

        var policyResult = new PolicySaleBuilder()
            .WithToday(new DateOnly(2027, 1, 1))
            .WithStartDate(startDate)
            .WithAmount(2.01m)
            .Sell();

        Assert.True(policyResult.IsSuccess);

        var result = policyResult.Value
            .CalculateCancellationQuote(
                startDate.AddDays(182));

        Assert.True(result.IsSuccess);
        Assert.Equal(
            1.01m,
            result.Value.RefundAmount);
    }

    [Fact]
    public void Cancel_WhenPolicyIsActive_ChangesStatusAndRecordsCancellationDate()
    {
        var startDate = new DateOnly(2026, 2, 1);
        var cancellationDate = startDate.AddDays(-1);

        var policyResult = new PolicySaleBuilder()
            .WithToday(new DateOnly(2026, 1, 1))
            .WithStartDate(startDate)
            .Sell();

        Assert.True(policyResult.IsSuccess);

        var policy = policyResult.Value;

        var result = policy.Cancel(
            cancellationDate,
            refundReference: "REF-000001");

        Assert.True(result.IsSuccess);
        Assert.Same(policy, result.Value);
        Assert.Equal(
            PolicyStatus.Cancelled,
            policy.Status);
        Assert.Equal(
            cancellationDate,
            policy.CancellationDate);
    }

    [Fact]
    public void Cancel_WhenBeforeStartDate_CreatesFullRefundUsingOriginalPaymentMethod()
    {
        var startDate = new DateOnly(2026, 2, 1);

        var policyResult = new PolicySaleBuilder()
            .WithToday(new DateOnly(2026, 1, 1))
            .WithStartDate(startDate)
            .WithAmount(365m)
            .WithPaymentType(PaymentType.Cheque)
            .Sell();

        Assert.True(policyResult.IsSuccess);

        var result = policyResult.Value.Cancel(
            startDate.AddDays(-1),
            "REFUND-000001");

        Assert.True(result.IsSuccess);

        var refund = Assert.IsType<Refund>(
            result.Value.Refund);

        Assert.Equal(
            "REFUND-000001",
            refund.Reference);

        Assert.Equal(
            PaymentType.Cheque,
            refund.Type);

        Assert.Equal(
            365m,
            refund.Amount);
    }

    [Fact]
    public void Cancel_WhenPolicyIsAlreadyCancelled_ReturnsFailureAndPreservesExistingCancellation()
    {
        var startDate = new DateOnly(2026, 2, 1);
        var firstCancellationDate = startDate.AddDays(-1);

        var policyResult = new PolicySaleBuilder()
            .WithToday(new DateOnly(2026, 1, 1))
            .WithStartDate(startDate)
            .Sell();

        Assert.True(policyResult.IsSuccess);

        var policy = policyResult.Value;

        var firstResult = policy.Cancel(
            firstCancellationDate,
            "REFUND-000001");

        Assert.True(firstResult.IsSuccess);

        var secondResult = policy.Cancel(
            startDate,
            "REFUND-000002");

        Assert.True(secondResult.IsFailure);

        var error = Assert.Single(
            secondResult.Errors);

        Assert.Equal(
            "policy.already_cancelled",
            error.Code);

        Assert.Equal(
            firstCancellationDate,
            policy.CancellationDate);

        Assert.Equal(
            "REFUND-000001",
            policy.Refund?.Reference);
    }

    [Fact]
    public void CalculateCancellationQuote_WhenPolicyHasClaims_ReturnsNoRefund()
    {
        var startDate = new DateOnly(2026, 2, 1);

        var policyResult = new PolicySaleBuilder()
            .WithToday(new DateOnly(2026, 1, 1))
            .WithStartDate(startDate)
            .WithAmount(365m)
            .WithClaims()
            .Sell();

        Assert.True(policyResult.IsSuccess);

        var result = policyResult.Value
            .CalculateCancellationQuote(
                startDate.AddDays(30));

        Assert.True(result.IsSuccess);
        Assert.Equal(
            0m,
            result.Value.RefundAmount);
    }

    [Fact]
    public void Cancel_WhenPolicyHasClaims_CancelsWithoutCreatingRefund()
    {
        var startDate = new DateOnly(2026, 2, 1);
        var cancellationDate = startDate.AddDays(30);

        var policyResult = new PolicySaleBuilder()
            .WithToday(new DateOnly(2026, 1, 1))
            .WithStartDate(startDate)
            .WithClaims()
            .Sell();

        Assert.True(policyResult.IsSuccess);

        var result = policyResult.Value.Cancel(
            cancellationDate,
            refundReference: null);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            PolicyStatus.Cancelled,
            result.Value.Status);

        Assert.Equal(
            cancellationDate,
            result.Value.CancellationDate);

        Assert.Null(result.Value.Refund);
    }

    [Fact]
    public void Cancel_WhenRefundIsDueAndReferenceIsMissing_ReturnsFailureWithoutChangingPolicy()
    {
        var startDate = new DateOnly(2026, 2, 1);

        var policyResult = new PolicySaleBuilder()
            .WithToday(new DateOnly(2026, 1, 1))
            .WithStartDate(startDate)
            .Sell();

        Assert.True(policyResult.IsSuccess);

        var policy = policyResult.Value;

        var result = policy.Cancel(
            startDate.AddDays(-1),
            refundReference: null);

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "policy.refund_reference.required",
            error.Code);

        Assert.Equal(
            PolicyStatus.Active,
            policy.Status);

        Assert.Null(policy.CancellationDate);
        Assert.Null(policy.Refund);
    }

    #endregion

    #region Renewal
    [Fact]
    public void Renew_WhenExactlyThirtyDaysBeforeEndDate_ExtendsPolicyByOneYear()
    {
        var policyResult = new PolicySaleBuilder()
            .WithToday(new DateOnly(2026, 1, 1))
            .WithStartDate(new DateOnly(2026, 2, 1))
            .Sell();

        Assert.True(policyResult.IsSuccess);

        var policy = policyResult.Value;
        var originalEndDate = policy.EndDate;
        var renewalDate = originalEndDate.AddDays(-30);

        var result = policy.Renew(
            renewalDate,
            paymentReference: "PAY-000002");

        Assert.True(result.IsSuccess);
        Assert.Same(policy, result.Value);

        Assert.Equal(
            originalEndDate.AddYears(1),
            policy.EndDate);
    }


    [Fact]
    public void Renew_WhenThirtyOneDaysBeforeEndDate_ReturnsTooEarlyWithoutChangingPolicy()
    {
        var policyResult = new PolicySaleBuilder()
            .Sell();

        Assert.True(policyResult.IsSuccess);

        var policy = policyResult.Value;
        var originalEndDate = policy.EndDate;

        var result = policy.Renew(
            originalEndDate.AddDays(-31),
            paymentReference: "PAY-000002");

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "policy.renewal.too_early",
            error.Code);

        Assert.Equal(
            originalEndDate,
            policy.EndDate);

        Assert.Single(policy.Payments);
    }

    [Fact]
    public void Renew_WhenAfterEndDate_ReturnsFailureWithoutChangingPolicy()
    {
        var policyResult = new PolicySaleBuilder()
            .Sell();

        Assert.True(policyResult.IsSuccess);

        var policy = policyResult.Value;
        var originalEndDate = policy.EndDate;

        var result = policy.Renew(
            originalEndDate.AddDays(1),
            paymentReference: "PAY-000002");

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "policy.renewal.after_expiry",
            error.Code);

        Assert.Equal(
            originalEndDate,
            policy.EndDate);

        Assert.Single(policy.Payments);
    }

    [Fact]
    public void Renew_WhenAutoRenewIsEnabled_CreatesPaymentUsingOriginalMethod()
    {
        var policyResult = new PolicySaleBuilder()
            .WithAmount(350.50m)
            .WithPaymentType(PaymentType.DirectDebit)
            .WithAutoRenew(true)
            .Sell();

        Assert.True(policyResult.IsSuccess);

        var policy = policyResult.Value;

        var result = policy.Renew(
            policy.EndDate.AddDays(-30),
            paymentReference: "PAY-000002");

        Assert.True(result.IsSuccess);
        Assert.Equal(2, policy.Payments.Count);

        var renewalPayment = policy.Payments[^1];

        Assert.Equal(
            "PAY-000002",
            renewalPayment.Reference);

        Assert.Equal(
            PaymentType.DirectDebit,
            renewalPayment.Type);

        Assert.Equal(
            350.50m,
            renewalPayment.Amount);
    }

    [Fact]
    public void Renew_WhenAutoRenewIsDisabled_ExtendsPolicyWithoutCreatingPayment()
    {
        var policyResult = new PolicySaleBuilder()
            .WithAutoRenew(false)
            .Sell();

        Assert.True(policyResult.IsSuccess);

        var policy = policyResult.Value;
        var originalEndDate = policy.EndDate;

        var result = policy.Renew(
            originalEndDate.AddDays(-30),
            paymentReference: null);

        Assert.True(result.IsSuccess);

        Assert.Equal(
            originalEndDate.AddYears(1),
            policy.EndDate);

        Assert.Single(policy.Payments);
    }

    [Fact]
    public void Renew_WhenAutoRenewUsesCheque_ReturnsFailureWithoutChangingPolicy()
    {
        var policyResult = new PolicySaleBuilder()
            .WithAutoRenew(true)
            .WithPaymentType(PaymentType.Cheque)
            .Sell();

        Assert.True(policyResult.IsSuccess);

        var policy = policyResult.Value;
        var originalEndDate = policy.EndDate;

        var result = policy.Renew(
            originalEndDate.AddDays(-30),
            paymentReference: "PAY-000002");

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "policy.renewal.cheque_not_supported",
            error.Code);

        Assert.Equal(
            originalEndDate,
            policy.EndDate);

        Assert.Single(policy.Payments);
    }

    [Fact]
    public void Renew_WhenPolicyIsCancelled_ReturnsFailureWithoutChangingPolicy()
    {
        var policyResult = new PolicySaleBuilder()
            .Sell();

        Assert.True(policyResult.IsSuccess);

        var policy = policyResult.Value;

        var cancellationResult = policy.Cancel(
            policy.StartDate.AddDays(-1),
            refundReference: "REFUND-000001");

        Assert.True(cancellationResult.IsSuccess);

        var originalEndDate = policy.EndDate;

        var renewalResult = policy.Renew(
            originalEndDate.AddDays(-30),
            paymentReference: "PAY-000002");

        Assert.True(renewalResult.IsFailure);

        var error = Assert.Single(
            renewalResult.Errors);

        Assert.Equal(
            "policy.renewal.cancelled",
            error.Code);

        Assert.Equal(
            originalEndDate,
            policy.EndDate);

        Assert.Single(policy.Payments);
    }
    #endregion

}
