using AuthService.Api.DTO;
using AuthService.Application.Commands;
using AuthService.Infrastructure;
using MediatR;
using Common;
using Microsoft.Extensions.Options;

namespace AuthService.Api.Endpoints;

public static class RefreshTokens
{
    public static IEndpointRouteBuilder MapRefreshTokensEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/refresh", async (
                HttpContext httpContext,
                IMediator mediator,
                IOptions<JwtOptions> jwtOptions,
                CancellationToken ct) =>
            {
                if (!httpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
                    return Error.Unauthorized("Отсутствует RefreshToken").ToProblemDetails();

                var command = new GetRefreshTokenCommand
                {
                    RefreshToken = refreshToken
                };

                var result = await mediator.Send(command, ct);

                if (!result.IsSuccess)
                {
                    httpContext.Response.Cookies.Delete("refreshToken", new CookieOptions
                    {
                        Path = "/api/auth"
                    });

                    return result.Error!.ToProblemDetails();
                }

                var tokens = result.Value!;

                httpContext.Response.Cookies.Append("refreshToken", tokens.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = httpContext.Request.IsHttps,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(jwtOptions.Value.RefreshExpiresHours),
                    Path = "/api/auth"
                });

                return Results.Ok(new { accessToken = tokens.AccessToken });
            })
            .Produces<RefreshResponseDto>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Обновить токены")
            .WithDescription("Читает refresh token из cookie, возвращает новый access token")
            .AllowAnonymous();

        return app;
    }
}