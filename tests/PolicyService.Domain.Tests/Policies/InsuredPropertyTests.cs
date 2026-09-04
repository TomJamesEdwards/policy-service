using PolicyService.Domain.Policies;

namespace PolicyService.Domain.Tests.Policies;

public sealed class InsuredPropertyTests
{
    [Fact]
    public void Create_WithValidDetails_ReturnsInsuredProperty()
    {
        var result = InsuredProperty.Create(
            addressLine1: "1 Test Street",
            addressLine2: "Test Village",
            addressLine3: null,
            postcode: "CH7 1AA");

        Assert.True(result.IsSuccess);
        Assert.Equal("1 Test Street", result.Value.AddressLine1);
        Assert.Equal("Test Village", result.Value.AddressLine2);
        Assert.Null(result.Value.AddressLine3);
        Assert.Equal("CH7 1AA", result.Value.Postcode);
    }

    [Fact]
    public void Create_WhenAddressLineOneIsWhitespace_ReturnsValidationFailure()
    {
        var result = InsuredProperty.Create(
            addressLine1: " ",
            addressLine2: null,
            addressLine3: null,
            postcode: "CH7 1AA");

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "property.address_line_1.required",
            error.Code);

        Assert.Equal(
            "Address line 1 is required.",
            error.Message);
    }

    [Fact]
    public void Create_WhenPostcodeIsWhitespace_ReturnsValidationFailure()
    {
        var result = InsuredProperty.Create(
            addressLine1: "1 Test Street",
            addressLine2: null,
            addressLine3: null,
            postcode: " ");

        Assert.True(result.IsFailure);

        var error = Assert.Single(result.Errors);

        Assert.Equal(
            "property.postcode.required",
            error.Code);

        Assert.Equal(
            "Postcode is required.",
            error.Message);
    }
}