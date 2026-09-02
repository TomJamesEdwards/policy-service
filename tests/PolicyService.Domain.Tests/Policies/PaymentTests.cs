using PolicyService.Domain.Policies;
using Xunit;

namespace PolicyService.Domain.Tests.Policies;

public sealed class PaymentTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WhenAmountIsNotPositive_ReturnsValidationFailure(
        decimal amount)
    {
        var result = Payment.Create(
            reference: "PAY-000001",
            type: PaymentType.DirectDebit,
            amount: amount);

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "payment.amount.must_be_positive",
            error.Code);

        Assert.Equal(
            "Payment amount must be greater than zero.",
            error.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WhenReferenceIsBlank_ReturnsValidationFailure(
    string reference)
    {
        var result = Payment.Create(
            reference,
            PaymentType.DirectDebit,
            250m);

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "payment.reference.required",
            error.Code);

        Assert.Equal(
            "Payment reference is required.",
            error.Message);
    }

    [Fact]
    public void Create_WhenCardNumberFailsLuhnValidation_ReturnsValidationFailure()
    {
        var result = Payment.Create(
            reference: "PAY-000001",
            type: PaymentType.Card,
            amount: 250m,
            cardNumber: "4111111111111112");

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "payment.card_number.invalid",
            error.Code);

        Assert.Equal(
            "Card number is invalid.",
            error.Message);
    }

    [Fact]
    public void Create_WhenCardNumberPassesLuhnValidation_ReturnsSuccessfulPayment()
    {
        var result = Payment.Create(
            reference: "PAY-000001",
            type: PaymentType.Card,
            amount: 250m,
            cardNumber: "4111111111111111");

        Assert.True(result.IsSuccess);

        Assert.Equal(PaymentType.Card, result.Value.Type);
    }
}