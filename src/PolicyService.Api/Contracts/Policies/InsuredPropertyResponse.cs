namespace PolicyService.Api.Contracts.Policies;

public sealed record InsuredPropertyResponse(
    string AddressLine1,
    string? AddressLine2,
    string? AddressLine3,
    string Postcode);