namespace PolicyService.Domain.Common;

public static class Result
{
    public static Result<T> Success<T>(T value)
        where T : class =>
        new(value);
}

public sealed class Result<T> where T : class
{
    private readonly T? _value;

    internal Result(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        IsSuccess = true;
        _value = value;
    }

    public bool IsSuccess { get; }

    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException(
                "A failed result does not contain a value.");
}