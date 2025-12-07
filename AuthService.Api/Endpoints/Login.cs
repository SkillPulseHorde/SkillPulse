using AuthService.Api.DTO;
using AuthService.Application.Commands;
using AuthService.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthService.Api.Endpoints;

public static class Login
{
    public static IEndpointRouteBuilder MapLoginEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (
                [FromBody] LoginRequest request,
                HttpContext httpContext,
                IMediator mediator,
                IOptions<JwtOptions> jwtOptions,
                CancellationToken ct) =>
            {
                var command = new AuthenticateUserCommand(request.Email, request.Password);

                var result = await mediator.Send(command, ct);

                if (!result.IsSuccess)
                    return result.Error!.ToProblemDetails();

                var loginResponseModel = result.Value!;

                httpContext.Response.Cookies.Append(
                    "refreshToken", loginResponseModel.TokenResponse.RefreshToken,
                    new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = httpContext.Request.IsHttps,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(jwtOptions.Value.RefreshExpiresHours),
                        Path = "/api/auth"
                    }
                );

                return Results.Ok(loginResponseModel.ToResponseDto());
            })
            .Produces<LoginResponseDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Войти в систему")
            .WithDescription("Возвращает access token и записывает refresh token в HttpOnly cookie")
            .AllowAnonymous();

        return app;
    }
}