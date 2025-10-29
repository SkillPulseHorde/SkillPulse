using System.Text;
using System.Text.Json.Serialization;
using AuthService.Api.DTO;
using AuthService;
using AuthService.Application.Commands;
using AuthService.Application.interfaces;
using AuthService.Domain.Repos;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Db;
using AuthService.Infrastructure.Repos;
using AuthService.Infrastructure.Http.ServiceClientOptions;
using AuthService.Infrastructure.Http.ServiceCollectionExtensions;
using Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MediatR.Extensions.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Common.Shared.Auth.Extensions;
using Microsoft.OpenApi.Models;

#region di
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDb")));
 
builder.Services.Configure<JwtOptions>(option =>
{
    option.SecretKey = builder.Configuration["JWT_SECRET_KEY"]
                       ?? throw new InvalidOperationException("JWT_SECRET_KEY не найден");
    if (Encoding.UTF8.GetByteCount(option.SecretKey) < 32)
        throw new InvalidOperationException("JWT_SECRET_KEY должен быть не менее 32 символа");

    option.AccessExpiresMinutes = int.TryParse(builder.Configuration["JWT_ACCESS_EXPIRES_MINUTES"], out var accMinutes)
        ? accMinutes
        : throw new InvalidOperationException("JWT_ACCESS_EXPIRES_HOURS не найден");

    option.RefreshExpiresHours = int.TryParse(builder.Configuration["JWT_REFRESH_EXPIRES_HOURS"], out var refHours)
        ? refHours
        : throw new InvalidOperationException("JWT_REFRESH_EXPIRES_HOURS не найден");
});

builder.Services.Configure<UserServiceOptions>(builder.Configuration);
builder.Services.AddUserServiceClient(builder.Configuration);


builder.Services.AddJwtAuthentication(options =>
{
    options.SecretKey = builder.Configuration["JWT_SECRET_KEY"] ?? "";
});
builder.Services.AddRoleBasedAuthorization();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();

//TODO заменить на нормальную валидацию
builder.Services.AddScoped<IValidator<CreateRegistrationCommand>, CreateRegistrationCommandValidator>();
builder.Services.AddScoped<IValidator<AuthenticateUserCommand>, AuthenticateUserCommandValidator>();
builder.Services.AddScoped<IValidator<GetRefreshTokenCommand>, GetRefreshTokenCommandValidator>();
builder.Services.AddScoped<IValidator<LogoutUserCommand>, LogoutUserCommandValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AuthService API",
        Version = "v1",
        Description = "API для аутентификации и авторизации"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Введите JWT токен в формате: Bearer {ваш_токен}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(typeof(CreateRegistrationCommandHandler).Assembly);
});

builder.Services.AddFluentValidation([typeof(CreateRegistrationCommandHandler).Assembly]);

builder.Services.ConfigureHttpJsonOptions(o => { o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()); });

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