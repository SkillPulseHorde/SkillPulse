namespace UserService.Application;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public List<string> Errors { get; } = [];

    private Result(T? value, bool isSuccess, List<string>? errors = null)
    {
        Value = value;
        IsSuccess = isSuccess;
        if (errors != null) Errors.AddRange(errors);
    }

    public static Result<T> Success(T value) => new(value, true);
    public static Result<T> Failure(params string[] errors) => new(default, false, errors.ToList());
}