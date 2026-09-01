namespace PolicyService.Domain.Policies;

public sealed class Policyholder
{
    public Policyholder(
        string firstName,
        string lastName,
        DateOnly dateOfBirth)
    {
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
    }

    public string FirstName { get; }

    public string LastName { get; }

    public DateOnly DateOfBirth { get; }
}