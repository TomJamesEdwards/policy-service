using PolicyService.Domain.Common;

namespace PolicyService.Domain.Policies;

public sealed class InsuredProperty
{
    private InsuredProperty(
        string addressLine1,
        string? addressLine2,
        string? addressLine3,
        string postcode)
    {
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        AddressLine3 = addressLine3;
        Postcode = postcode;
    }

    public string AddressLine1 { get; }

    public string? AddressLine2 { get; }

    public string? AddressLine3 { get; }

    public string Postcode { get; }

    public static Result<InsuredProperty> Create(
        string? addressLine1,
        string? addressLine2,
        string? addressLine3,
        string? postcode)
    {
        if (string.IsNullOrWhiteSpace(addressLine1))
        {
            return Result.Failure<InsuredProperty>(
                InsuredPropertyErrors.AddressLineOneRequired);
        }

        if (string.IsNullOrWhiteSpace(postcode))
        {
            return Result.Failure<InsuredProperty>(
                InsuredPropertyErrors.PostcodeRequired);
        }

        return Result.Success(
            new InsuredProperty(
                addressLine1,
                addressLine2,
                addressLine3,
                postcode ?? string.Empty));
    }
}