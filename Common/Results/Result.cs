namespace JWTAuthentication.Common.Results;

public sealed class Result<T>
{
    private Result(T value)
    {
        Value = value;
    }

    private Result(Error error)
    {
        Error = error;
    }

    public T Value { get; }
    public Error Error { get; }
    public bool IsSuccess => Error is null;

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error ?? throw new ArgumentNullException(nameof(error)));

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}
