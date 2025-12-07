using Common;

namespace RecommendationService.Application;

public static class ExceptionToErrorMapper
{
    public static Error Map(Exception ex)
    {
        return ex switch
        {
            Exceptions.AiAuthException => Error.BadGateway(ex.Message),
            Exceptions.AiTransientException => Error.ServiceUnavailable(ex.Message),
            Exceptions.AiInvalidResponseException => Error.BadGateway(ex.Message),
            TimeoutException => Error.GatewayTimeout(ex.Message),
            OperationCanceledException => Error.RequestTimeout(ex.Message),
            HttpRequestException => Error.BadGateway(ex.Message),
            _ => Error.InternalServerError(ex.Message)
        };
    }
}