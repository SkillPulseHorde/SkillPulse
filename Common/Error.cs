namespace Common;

public class Error
{
    public string Code { get; }
    
    public string Message { get; }

    public Dictionary<string, string[]>? ValidationErrors { get; }

    private Error(string code, string message, Dictionary<string, string[]>? validationErrors = null)
    {
        Code = code;
        Message = message;
        ValidationErrors = validationErrors;
    }

    public static Error NotFound(string msg) =>
        new($"not_found", msg);

    public static Error Validation(Dictionary<string, string[]> errors) =>
        new($"validation_error", "", errors);
    
    public static Error Validation(string msg) =>
        new($"validation_error", msg);
    
    public static Error Conflict(string msg) =>
        new($"conflict", msg);
    
    public static Error Unauthorized(string msg) =>
        new($"unauthorized", msg);
}