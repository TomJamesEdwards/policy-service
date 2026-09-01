namespace PolicyService.Domain.Policies;

public sealed class InsuredProperty
{
    public InsuredProperty(
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
}