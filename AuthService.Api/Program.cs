using AuthService.Api.DTO;
using AuthService.Api;
using AuthService.Api.Extensions.DependencyInjection;
using AuthService.Application.Commands;
using Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Common.Shared.Auth.Extensions;

#region di

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthenticationConfiguration(builder.Configuration);
builder.Services.AddRoleBasedAuthorization();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

#endregion

#region endpoints

app.MapPost("/api/auth/register", async ([FromBody] RegistrationRequest request, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new CreateRegistrationCommand(request.Email, request.Password), ct);

    return result.IsSuccess
        ? Results.Ok()
        : result.Error!.ToProblemDetails();
})
.Produces(statusCode: StatusCodes.Status200OK)
.AllowAnonymous()
.WithSummary("Зарегистрировать пользователя")
.WithDescription("Возвращает только статус-код");

app.MapPost("/api/auth/login", async ([FromBody] LoginRequest request, HttpContext httpContext, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new AuthenticateUserCommand(request.Email, request.Password), ct);

    if (!result.IsSuccess)
        return result.Error!.ToProblemDetails();

    var loginResponseModel = result.Value!;
    
    httpContext.Response.Cookies.Append("refreshToken", loginResponseModel.TokenResponse.RefreshToken, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.UtcNow.AddHours(int.Parse(builder.Configuration["JWT_REFRESH_EXPIRES_HOURS"]!)),
        Path = "/api/auth"
    });
    
    return Results.Ok(new
        {
            accessToken = loginResponseModel.TokenResponse.AccessToken,
            userId = loginResponseModel.UserId
        });
})
.Produces<LoginResponseDto>()
.WithSummary("Войти в систему")
.AllowAnonymous();

app.MapPost("/api/auth/refresh", async (HttpContext httpContext, IMediator mediator, CancellationToken ct) =>
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
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.UtcNow.AddHours(int.Parse(builder.Configuration["JWT_REFRESH_EXPIRES_HOURS"]!)),
        Path = "/api/auth"
    });
    
    return Results.Ok(new { accessToken = tokens.AccessToken });
})
.Produces<RefreshResponseDto>()
.WithSummary("Обновить токены")
.WithDescription("Читает refresh token из cookie, возвращает новый access token")
.AllowAnonymous();

app.MapPost("/api/auth/logout", async (HttpContext httpContext, IMediator mediator, CancellationToken ct) =>
    {
    var userId = httpContext.User.GetUserId();
    
    var result = await mediator.Send(new LogoutUserCommand(userId), ct);

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
.Produces(statusCode: StatusCodes.Status200OK)  
.WithSummary("Выйти из системы")
.WithDescription("Удаляет refresh token и инвалидирует сессию")
.RequireAuthorization("Authenticated");

#endregion

app.Run();