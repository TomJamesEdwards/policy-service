namespace PolicyService.Application.Policies.Sell;

public sealed record InsuredPropertyInput(
    string? AddressLine1,
    string? AddressLine2,
    string? AddressLine3,
    string? Postcode);