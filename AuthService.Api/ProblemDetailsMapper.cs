using Common;

namespace AuthService;

public static class ProblemDetailsMapper
{
    public static IResult ToProblemDetails(this Error error)
    {
        var statusCode = error.Code switch
        {
            "not_found" => StatusCodes.Status404NotFound,
            "validation_error" => StatusCodes.Status400BadRequest,
            "unauthorized" => StatusCodes.Status401Unauthorized,
            "conflict" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(
            title: error.Code.Replace("_", " ").ToUpperInvariant(),
            detail: error.Message,
            statusCode: statusCode
        );
    }
}