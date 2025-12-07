using AuthService.Application.Commands;
using Common.Shared.Auth.Extensions;
using MediatR;

namespace AuthService.Api.Endpoints;

public static class Logout
{
    public static IEndpointRouteBuilder MapLogoutEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/logout", async (
                HttpContext httpContext,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var userId = httpContext.User.GetUserId();

                var command = new LogoutUserCommand(userId);

                var result = await mediator.Send(command, ct);

                if (result.IsSuccess)
                {
                    httpContext.Response.Cookies.Delete("refreshToken", new CookieOptions
                    {
                        Path = "/api/auth"
                    });
                }

                return result.IsSuccess
                    ? Results.Ok()
                    : result.Error!.ToProblemDetails();
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Выйти из системы")
            .WithDescription("Удаляет refresh token и инвалидирует сессию")
            .RequireAuthorization("Authenticated");

        return app;
    }
}