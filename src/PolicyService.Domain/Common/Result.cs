namespace PolicyService.Domain.Common;

public static class Result
{
    public static Result<T> Success<T>(T value)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Result<T>(
            value,
            Array.Empty<DomainError>());
    }

    public static Result<T> Failure<T>(
    IReadOnlyCollection<DomainError> errors)
    where T : class
    {
        ArgumentNullException.ThrowIfNull(errors);

        return new Result<T>(
            value: null,
            errors);
    }

    public static Result<T> Failure<T>(params DomainError[] errors)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(errors);

        return new Result<T>(
            value: null,
            errors);
    }
}

public sealed class Result<T> where T : class
{
    private readonly T? _value;

    internal Result(
        T? value,
        IReadOnlyCollection<DomainError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var hasValue = value is not null;
        var hasErrors = errors.Count > 0;

        if (hasValue && hasErrors)
        {
            throw new ArgumentException(
                "A successful result cannot contain errors.",
                nameof(errors));
        }

        if (!hasValue && !hasErrors)
        {
            throw new ArgumentException(
                "A failed result must contain at least one error.",
                nameof(errors));
        }

        IsSuccess = hasValue;
        _value = value;
        Errors = errors.ToList().AsReadOnly();
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException(
                "A failed result does not contain a value.");

    public IReadOnlyList<DomainError> Errors { get; }
}