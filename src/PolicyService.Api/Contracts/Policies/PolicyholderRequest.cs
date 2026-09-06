namespace PolicyService.Api.Contracts.Policies;

public sealed record PolicyholderRequest(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth);