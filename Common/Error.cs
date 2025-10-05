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
    
    // Дальше можно добавлять свои переиспользуемые ошибки [[убрать коммент при сдаче]]
}