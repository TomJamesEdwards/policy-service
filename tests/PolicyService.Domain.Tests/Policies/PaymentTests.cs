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
}