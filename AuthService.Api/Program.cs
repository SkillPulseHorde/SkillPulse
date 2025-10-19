using System.Text;
using System.Text.Json.Serialization;
using AuthService.Api.DTO;
using AuthService;
using AuthService.Application.Commands;
using AuthService.Application.interfaces;
//using AuthService.Application.Queries;
using AuthService.Domain.Repos;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Db;
using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using MediatR.Extensions.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

#region di

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDb")));

builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<JwtOptions>(option => {
    option.SecretKey = builder.Configuration["JWT_SECRET_KEY"]
        ?? throw new InvalidOperationException("Invalid JWT_SECRET_KEY value");
    
    option.AccessExpiresHours = int.TryParse(builder.Configuration["JWT_ACCESS_EXPIRES_HOURS"], out var accHours)
        ?  accHours
        : throw new InvalidOperationException("Invalid JWT_ACCESS_EXPIRES_HOURS value");
    
    option.RefreshExpiresHours = int.TryParse(builder.Configuration["JWT_REFRESH_EXPIRES_HOURS"], out var refHours)
        ?  refHours
        : throw new InvalidOperationException("Invalid JWT_REFRESH_EXPIRES_HOURS value");
});

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuer = false,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWT_SECRET_KEY"])),
    };
}); 

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();

builder.Services.AddScoped<IValidator<CreateRegistrationCommand>, CreateRegistrationCommandValidator>();//TODO заменить на нормальную валидацию
builder.Services.AddScoped<IValidator<AuthenticateUserCommand>, AuthenticateUserCommandValidator>();
builder.Services.AddScoped<IValidator<GetRefreshTokenCommand>, GetRefreshTokenCommandValidator>();
builder.Services.AddScoped<IValidator<LogoutUserCommand>, LogoutUserCommandValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssemblies(typeof(CreateRegistrationCommandHandler).Assembly);
});

builder.Services.AddFluentValidation([typeof(CreateRegistrationCommandHandler).Assembly]);

builder.Services.ConfigureHttpJsonOptions(o => 
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

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
        : result.Error.ToProblemDetails();
});

app.MapPost("/api/auth/login", async ([FromBody] LoginRequest request, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new AuthenticateUserCommand(request.Email, request.Password), ct);

    return result.IsSuccess
        ? Results.Ok(result.Value)
        : result.Error.ToProblemDetails();
});

app.MapPost("/api/auth/refresh", async ([FromBody] RefreshRequest request, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new GetRefreshTokenCommand(request.RefreshToken), ct);

    return result.IsSuccess
        ? Results.Ok(result.Value)
        : result.Error.ToProblemDetails();
}).RequireAuthorization();

app.MapPost("/api/auth/logout", async ([FromBody] LogoutRequest request, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new LogoutUserCommand(request.UserId), ct);

    return result.IsSuccess
        ? Results.Ok()
        : result.Error.ToProblemDetails();
}).RequireAuthorization();
#endregion

app.Run();