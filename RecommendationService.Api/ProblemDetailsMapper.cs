using Common;

namespace RecommendationService.Api;

public static class ProblemDetailsMapper
{
    public static IResult ToProblemDetails(this Error error)
    {
        var statusCode = error.Code switch
        {
            "not_found" => StatusCodes.Status404NotFound,
            "validation_error" => StatusCodes.Status422UnprocessableEntity,
            "unauthorized" => StatusCodes.Status401Unauthorized,
            "conflict" => StatusCodes.Status409Conflict,
            "bad_gateway" => StatusCodes.Status502BadGateway,
            "service_unavailable" => StatusCodes.Status503ServiceUnavailable,
            "request_timeout" => StatusCodes.Status408RequestTimeout,
            "gateway_timeout" => StatusCodes.Status504GatewayTimeout,
            _ => StatusCodes.Status500InternalServerError
        };

        var errorTitle = error.Code.Replace("_", " ").ToUpperInvariant();

        if (error.ValidationErrors is not null)
        {
            return Results.ValidationProblem(
                title: errorTitle,
                errors: error.ValidationErrors,
                statusCode: statusCode
            );
        }

        return Results.Problem(
            title: errorTitle,
            detail: error.Message,
            statusCode: statusCode
        );
    }
}