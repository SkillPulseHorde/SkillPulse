namespace AssessmentService.Api.Middleware;

public class RequireInternalRoleFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var user = context.HttpContext.User;
        if (!user.IsInRole("internal"))
            return Results.Unauthorized();

        return await next(context);
    }
}