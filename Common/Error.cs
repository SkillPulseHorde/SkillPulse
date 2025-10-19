namespace Common;

public class Error
{
    public string Code { get; }
    public string Message { get; }

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public static Error NotFound(string msg) =>
        new($"not_found", msg);

    public static Error Validation(string msg) =>
        new($"validation_error", msg);
    
    public static Error Conflict(string msg) =>
        new($"conflict", msg);
    
    public static Error Unauthorized(string msg) =>
        new($"unauthorized", msg);
}