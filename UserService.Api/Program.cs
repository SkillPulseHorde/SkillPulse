using System.Text.Json.Serialization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserService.Application;
using UserService.Application.Queries;
using UserService.Domain.Repos;
using UserService.Infrastructure;
using UserService.Infrastructure.Db;

#region di
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("UserDb")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserServiceApp>();

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
app.MapGet("/api/users/{id}", async (IMediator mediator, Guid id, CancellationToken ct) =>
{
    if (id == Guid.Empty)
        return Results.BadRequest(new { error = "Id must be a non-empty GUID." });
    
    var result = await mediator.Send(new GetUserByIdQuery(id), ct);
    
    return result.IsSuccess 
        ? Results.Ok(result.Value) 
        : Results.BadRequest(new 
            { Error = new { Error = string.Join("\n", result.Errors) } }); //TODO добавить разграничение ошибок (SP-24)
});

app.MapGet("/api/users/{id}/subordinates", async (IMediator mediator, Guid id, CancellationToken ct) =>
{
    if (id == Guid.Empty)
        return Results.BadRequest(new { error = "Id must be a non-empty GUID." });
    
    var result = await mediator.Send(new GetSubordinatesByUserIdQuery(id), ct);
    
    return result.IsSuccess 
        ? Results.Ok(result.Value)
        : Results.BadRequest(new 
            { Error = new { Error = string.Join("\n", result.Errors) } });
});
#endregion

app.Run();