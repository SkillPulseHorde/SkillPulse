using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService;
using UserService.Application.Models;
using UserService.Application.Queries;
using UserService.Domain.Repos;
using UserService.Dto;
using UserService.Infrastructure.Db;
using UserService.Infrastructure.Repos;

#region di
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("UserDb")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
//builder.Services.AddValidatorsFromAssembly(Assembly.Load("MyProject.Application"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
#endregion

#region endpoints
app.MapGet("/api/users/{id:guid}", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new GetUserByIdQuery(id), ct);
    
    return result.IsSuccess 
        ? Results.Ok(result.Value) 
        : result.Error!.ToProblemDetails();
})
.Produces<UserModel>()
.WithSummary("Получить пользователя по ID");


app.MapGet("/api/users", async (
        [FromQuery] Guid currentUserId, 
        IMediator mediator,
        CancellationToken ct) =>
    {
        var result = await mediator.Send(new GetAllUsersQuery(currentUserId), ct);
    
        return result.IsSuccess 
            ? Results.Ok(result.Value.Select(u => u.ToDto()).ToList()) 
            : result.Error!.ToProblemDetails();
    })
    .Produces<List<UserShortInfoDto>>()
    .WithSummary("Получить всех пользователей, начиная со своей команды");


app.MapGet("/api/users/{email}/id", async (string email, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new GetUserIdByEmailQuery(email), ct);
    
    return result.IsSuccess 
        ? Results.Ok(result.Value) 
        : result.Error!.ToProblemDetails();
})
.Produces<Guid>()
.WithSummary("Получить идентификатор пользователя по email")
.WithDescription("Возвращает только GUID пользователя.");


app.MapGet("/api/users/{id:guid}/subordinates", async (Guid id, IMediator mediator, CancellationToken ct) =>
{
    var result = await mediator.Send(new GetSubordinatesByUserIdQuery(id), ct);

    return result.IsSuccess
        ? Results.Ok(result.Value)
        : result.Error!.ToProblemDetails();
})
.Produces<List<UserModel>>()
.WithSummary("Получить подчиненных пользователя по его ID");

app.MapPost("/api/users/exist", async (
        [FromBody] CheckUsersExistRequestDto request, 
        IMediator mediator,
        CancellationToken ct) =>
{
    var result = await mediator.Send(new AreUsersExistQuery(request.UserIds), ct);

    return result.IsSuccess
        ? Results.Ok(result.Value)
        : result.Error!.ToProblemDetails();
})
.Produces<bool>()
.WithSummary("Проверить существование пользователей по списку ID");
#endregion

app.Run();