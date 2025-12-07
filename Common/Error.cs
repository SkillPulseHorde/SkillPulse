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
        new("not_found", msg);

    public static Error Validation(Dictionary<string, string[]> errors) =>
        new("validation_error", "", errors);

    public static Error Validation(string msg) =>
        new("validation_error", msg);

    public static Error Conflict(string msg) =>
        new("conflict", msg);

    public static Error Unauthorized(string msg) =>
        new("unauthorized", msg);

    public static Error Forbidden(string msg) =>
        new("forbidden", msg);

    public static Error BadGateway(string msg) =>
        new("bad_gateway", msg);

    public static Error ServiceUnavailable(string msg) =>
        new("service_unavailable", msg);

    public static Error RequestTimeout(string msg) =>
        new("request_timeout", msg);

    public static Error GatewayTimeout(string msg) =>
        new("gateway_timeout", msg);

    public static Error InternalServerError(string msg) =>
        new("internal_server_error", msg);
}