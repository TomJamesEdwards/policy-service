namespace PolicyService.Api.Contracts.Policies;

public sealed record InsuredPropertyRequest(
    string? AddressLine1,
    string? AddressLine2,
    string? AddressLine3,
    string? Postcode);