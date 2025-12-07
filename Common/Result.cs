namespace Common;

public class Result
{
    [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; private set; }

    public Error? Error { get; private set; }

    // Для фабрики
    protected Result()
    {
        IsSuccess = true;
        Error = null;
    }

    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);

    public static TResult Failure<TResult>(Error error) where TResult : Result, new()
    {
        return new TResult
        {
            IsSuccess = false,
            Error = error
        };
    }
}

public sealed class Result<T> : Result
{
    public T? Value { get; set; }

    [System.Diagnostics.CodeAnalysis.MemberNotNullWhen(true, nameof(Value))]
    public new bool IsSuccess => base.IsSuccess;

    public Result()
    {
        Value = default;
    }

    private Result(T value) : base(true, null)
    {
        Value = value;
    }

    private Result(Error error) : base(false, error)
    {
        Value = default;
    }

    public static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) =>
        Success(value);

    public static implicit operator Result<T>(Error error) =>
        Failure(error);
}