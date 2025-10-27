using System.Text.Json.Serialization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService;
using UserService.Application.Models;
using UserService.Application.Queries;
using UserService.Domain.Repos;
using UserService.Infrastructure.Db;
using UserService.Infrastructure.Repos;
using Common.Shared.Auth.Extensions;
using Microsoft.OpenApi.Models;
using UserService.Middleware;

#region di
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("UserDb")));

builder.Services.AddJwtAuthentication(options =>
{
    options.SecretKey = builder.Configuration["JWT_SECRET_KEY"] ?? "";
});
builder.Services.AddRoleBasedAuthorization();

builder.Services.AddScoped<IUserRepository, UserRepository>();
//builder.Services.AddValidatorsFromAssembly(Assembly.Load("MyProject.Application"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "UserService API",
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
    cfg.RegisterServicesFromAssemblies(typeof(GetUserByIdQueryHandler).Assembly);
});

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()); 
});


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<InternalAuthMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

#endregion

#region endpoints
app.MapGet("/api/users/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new GetUserByIdQuery(id), ct);
    
    return result.IsSuccess 
        ? Results.Ok(result.Value) 
        : result.Error.ToProblemDetails();
})
.RequireAuthorization()
.Produces<UserModel>()
.WithSummary("Получить пользователя по ID");


app.MapGet("/api/users/{email}/id", async (string email, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new GetUserIdByEmailQuery(email), ct);
    
    return result.IsSuccess 
        ? Results.Ok(result.Value) 
        : result.Error.ToProblemDetails();
})
.AddEndpointFilter<RequireInternalRoleFilter>()
.Produces<Guid>()
.WithSummary("Получить идентификатор пользователя по email")
.WithDescription("Возвращает только GUID пользователя.");


app.MapGet("/api/users/{id:guid}/subordinates", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new GetSubordinatesByUserIdQuery(id), ct);

    return result.IsSuccess
        ? Results.Ok(result.Value)
        : result.Error.ToProblemDetails();
})
.RequireAuthorization("HRAndManagers")
.Produces<List<UserModel>>()
.WithSummary("Получить подчиненных пользователя по его ID");
#endregion

app.Run();