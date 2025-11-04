using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AssessmentService.Api;
using AssessmentService.Api.Dto;
using AssessmentService.Application.Commands;
using AssessmentService.Application.Models;
using AssessmentService.Application.Queries;
using AssessmentService.Domain.Repos;
using AssessmentService.Infrastructure.Db;
using AssessmentService.Infrastructure.Http.ServiceClientOptions;
using AssessmentService.Infrastructure.Http.ServiceCollectionExtensions;
using AssessmentService.Infrastructure.Repos;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Common.Shared.Auth.Extensions;
using Microsoft.OpenApi.Models;

#region di

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AssessmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AssessmentDb")));

builder.Services.AddJwtAuthentication(options =>
{
    options.SecretKey = builder.Configuration["Jwt:SecretKey"]
                        ?? builder.Configuration["JWT_SECRET_KEY"]
                        ?? throw new InvalidOperationException("JWT SecretKey не найден");
});

builder.Services.AddRoleBasedAuthorization();

builder.Services.AddScoped<IAssessmentRepository, AssessmentRepository>();
builder.Services.AddScoped<IEvaluationRepository, EvaluationRepository>();

builder.Services.Configure<UserServiceOptions>(builder.Configuration);
builder.Services.AddUserServiceClient(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AssessmentService API",
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
    cfg.RegisterServicesFromAssemblies(typeof(CreateAssessmentCommandHandler).Assembly);
});

builder.Services.ConfigureHttpJsonOptions(o => { o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()); });


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

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
            EvaluatorIds = request.EvaluatorIds
        };

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.Error!.ToProblemDetails();
    })
    .Produces<Guid>()
    .WithSummary("Запустить аттестацию")
    .RequireAuthorization("HROnly");

app.MapGet("/api/assessments/{assessmentId:guid}", async (
        [FromRoute] Guid assessmentId,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var query = new GetAssessmentByIdQuery(assessmentId);

        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.ToDetailResponseDto())
            : result.Error!.ToProblemDetails();
    })
    .Produces<AssessmentDetailResponseDto>()
    .WithSummary("Получить аттестацию по ID")
    .RequireAuthorization("HROnly");

app.MapPut("/api/assessments/{assessmentId:guid}", async (
        [FromRoute] Guid assessmentId,
        [FromBody] UpdateAssessmentRequestDto request,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var command = new UpdateAssessmentCommand
        {
            AssessmentId = assessmentId,
            EndsAt = request.EndsAt,
            EvaluatorIds = request.EvaluatorIds
        };

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.Error!.ToProblemDetails();
    })
    .Produces<Guid>()
    .WithSummary("Обновить аттестацию")
    .RequireAuthorization("HROnly");

app.MapDelete("/api/assessments/{assessmentId:guid}", async (
        [FromRoute] Guid assessmentId,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var command = new DeleteAssessmentCommand(assessmentId);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.Error!.ToProblemDetails();
    })
    .Produces<Guid>()
    .WithSummary("Удалить аттестацию")
    .RequireAuthorization("HROnly");

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
    .WithSummary("Получить Id рецензентов пользователя по его ID")
    .RequireAuthorization("Authenticated");

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
    .WithSummary("Обновить список рецензентов пользователя (полная замена)")
    .RequireAuthorization("Authenticated");

app.MapGet("/api/assessments", async (
        [Required] [FromQuery] bool isActive,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var query = new GetAssessmentsQuery(isActive);

        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.Select(a => a.ToResponseDto()).ToList())
            : result.Error!.ToProblemDetails();
    })
    .Produces<List<AssessmentResponseDto>>()
    .WithSummary("Получить список аттестаций")
    .WithOpenApi(operation =>
    {
        operation.Parameters[0].Description = "true - активные аттестации, false - предстоящие аттестации";
        return operation;
    })
    .RequireAuthorization("HROnly");

#endregion

app.Run();