using MediatR;
using UserService.Extensions;
using Microsoft.AspNetCore.Mvc;
using UserService;
using UserService.Application.Models;
using UserService.Application.Queries;
using UserService.Dto;
using Common.Shared.Auth.Extensions;
using Common.Middleware;

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

app.UseMiddleware<ServiceAuthenticationMiddleware>(app.Configuration.GetSection("InternalToken").Value ??
                                                   throw new InvalidOperationException("InternalToken не был передан"));

app.UseAuthentication();

if (app.Environment.IsDevelopment())
    app.UseMiddleware<DevelopmentAuthenticationMiddleware>();

app.UseAuthorization();

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
    .ProducesProblem(StatusCodes.Status401Unauthorized)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .WithSummary("Получить пользователя по ID")
    .RequireAuthorization("AuthenticatedAndService");


app.MapGet("/api/users", async (
        [FromQuery] Guid currentUserId,
        [FromQuery] bool? includeCurrentUser,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var result = await mediator.Send(new GetAllUsersQuery(currentUserId, includeCurrentUser), ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.Select(u => u.ToDto()).ToList())
            : result.Error!.ToProblemDetails();
    })
    .WithOpenApi(operation =>
    {
        operation.Parameters[0].Description = "ID текущего пользователя";
        operation.Parameters[1].Description = "Включить текущего пользователя в результаты";
        return operation;
    })
    .Produces<List<UserShortInfoDto>>()
    .ProducesProblem(StatusCodes.Status401Unauthorized)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .WithSummary("Получить всех пользователей, начиная со своей команды")
    .RequireAuthorization("Authenticated");


app.MapGet("/api/users/{email}/id", async (string email, IMediator mediator, CancellationToken ct) =>
    {
        var result = await mediator.Send(new GetUserIdByEmailQuery(email), ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.Error!.ToProblemDetails();
    })
    .RequireAuthorization("ServiceOnly")
    .Produces<Guid>()
    .WithSummary("Получить идентификатор пользователя по email")
    .WithDescription("Возвращает только GUID пользователя.");
//.ExcludeFromDescription(); // Для отключения в сваггере


app.MapGet("/api/users/{id:guid}/subordinates", async (Guid id, IMediator mediator, CancellationToken ct) =>
    {
        var result = await mediator.Send(new GetSubordinatesByUserIdQuery(id), ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.Error!.ToProblemDetails();
    })
    .Produces<List<UserModel>>()
    .ProducesProblem(StatusCodes.Status401Unauthorized)
    .ProducesProblem(StatusCodes.Status403Forbidden)
    .ProducesProblem(StatusCodes.Status404NotFound)
    .WithSummary("Получить подчиненных пользователя по его ID")
    .RequireAuthorization("HRAndManagers");

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
    .RequireAuthorization("ServiceOnly")
    .Produces<bool>()
    .WithSummary("Проверить существование пользователей по списку ID");

app.MapPost("/api/users/by-ids", async (
        [FromBody] GetUsersByIdsRequestDto request,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var result = await mediator.Send(new GetUsersByIdsQuery(request.UserIds), ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.Select(u => u.ToDto()).ToList())
            : result.Error!.ToProblemDetails();
    })
    .RequireAuthorization("AuthenticatedAndService")
    .Produces<List<UserShortInfoDto>>()
    .WithSummary("Получить пользователей по списку ID");

#endregion

app.Run();