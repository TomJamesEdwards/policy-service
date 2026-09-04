namespace PolicyService.Application.Policies.Sell;

public sealed record PolicyholderInput(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth);