using System.Text.Json.Serialization;
using AssessmentService.Api;
using AssessmentService.Api.Dto;
using AssessmentService.Application.Commands;
using AssessmentService.Application.Queries;
using AssessmentService.Domain.Repos;
using AssessmentService.Infrastructure.Db;
using AssessmentService.Infrastructure.Http.ServiceClientOptions;
using AssessmentService.Infrastructure.Http.ServiceCollectionExtensions;
using AssessmentService.Infrastructure.Repos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

#region di
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AssessmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AssessmentDb")));

builder.Services.AddScoped<IAssessmentRepository, AssessmentRepository>();
builder.Services.AddScoped<IEvaluationRepository, EvaluationRepository>();

builder.Services.Configure<UserServiceOptions>(
    builder.Configuration.GetSection("UserService")
);

builder.Services.AddUserServiceClient(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(typeof(CreateAssessmentCommandHandler).Assembly);
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
app.MapPost("/api/assessments", async (
    [FromBody] CreateAssessmentRequestDto request,
    IMediator mediator,
    CancellationToken ct) =>
{
    var command = new CreateAssessmentCommand
    {
        EvaluateeId = request.EvaluateeId,
        StartAt = request.StartAt,
        EndsAt = request.EndsAt,
        CreatedByUserId = request.CreatedByUserId,
    };
    
    var result = await mediator.Send(command, ct);
    
    return result.IsSuccess 
        ? Results.Ok(result.Value) 
        : result.Error!.ToProblemDetails();
});

app.MapGet("/api/assessments/evaluators/{userId:guid}", async (
    [FromRoute] Guid userId,
    IMediator mediator,
    CancellationToken ct) =>
{
    var query = new GetEvaluatorIdsByUserIdQuery(userId);
    
    var result = await mediator.Send(query, ct);
    
    return result.IsSuccess 
        ? Results.Ok(result.Value) 
        : result.Error!.ToProblemDetails();
})
.Produces<List<Guid>>()
.WithSummary("Получить Id рецензентов пользователя по его ID");

// Новый эндпоинт: обновление списка рецензентов пользователя
app.MapPut("/api/assessments/evaluators/{userId:guid}", async (
    [FromRoute] Guid userId,
    [FromBody] UpdateEvaluatorsRequestDto request,
    IMediator mediator,
    CancellationToken ct) =>
{
    var command = new UpdateEvaluatorsForUserCommand
    {
        UserId = userId,
        EvaluatorIds = request.EvaluatorIds ?? new List<Guid>()
    };

    var result = await mediator.Send(command, ct);

    return result.IsSuccess
        ? Results.NoContent()
        : result.Error!.ToProblemDetails();
})
.Produces(StatusCodes.Status204NoContent)
.WithSummary("Обновить список рецензентов пользователя (полная замена)");

#endregion

app.Run();