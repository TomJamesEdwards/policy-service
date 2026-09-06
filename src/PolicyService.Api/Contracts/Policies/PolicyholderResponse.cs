namespace PolicyService.Api.Contracts.Policies;

public sealed record PolicyholderResponse(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth);