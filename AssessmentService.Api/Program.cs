using System.ComponentModel.DataAnnotations;
using AssessmentService.Api;
using AssessmentService.Api.Dto;
using AssessmentService.Api.Dto.Evaluation;
using AssessmentService.Application.Commands;
using AssessmentService.Application.Commands.CommandParameters;
using AssessmentService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Common.Shared.Auth.Extensions;
using AssessmentService.Api.Extensions.DependencyInjection;
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

if (app.Environment.IsDevelopment())
    app.UseMiddleware<DevelopmentAuthenticationMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

#endregion

#region endpoints

#region Assessments
// Запустить аттестацию
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

// Получить аттестацию по ID
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


// Обновить аттестацию
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


// Удалить аттестацию
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


// Получить Id рецензентов пользователя по его ID
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


// Обновить список рецензентов пользователя (полная замена)
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


// Получить список аттестаций
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


// Получить активные аттестации по ID рецензента
app.MapGet("/api/assessments/evaluator/{userId:guid}/active", async (
        [FromRoute] Guid userId,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var query = new GetActiveAssessmentsByEvaluatorIdQuery(userId);

        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.Select(a => a.ToAssessmentShortInfoResponseDto()).ToList())
            : result.Error!.ToProblemDetails();
    })
    .Produces<List<AssessmentShortInfoResponseDto>>()
    .WithSummary("Получить активные аттестации, назначенные рецензенту");
    //.RequireAuthorization("Authenticated"); //todo
#endregion

// Получить все компетенции
app.MapGet("/api/competences", async (IMediator mediator, CancellationToken ct) =>
    {
        var query = new GetAllCompetencesQuery();

        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value.Select(c => new CompetenceResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Criteria = c.Criteria.Select(cr => new CriterionResponseDto
                {
                    Id = cr.Id,
                    Name = cr.Name
                }).ToList()
            }).ToList())
            : result.Error!.ToProblemDetails();
    })
    .Produces<List<CompetenceResponseDto>>()
    .WithSummary("Получить все компетенции");
    //.RequireAuthorization("Authenticated"); //todo


// Отправить оценку
app.MapPost("/api/evaluations", async (
        [FromBody] CreateEvaluationRequestDto request,
        IMediator mediator,
        CancellationToken ct) =>
    {
        var command = new CreateEvaluationCommand
        {
            AssessmentId = request.AssessmentId,
            EvaluatorId = request.EvaluatorId,
            CompetenceEvaluations = request.CompetenceEvaluations.Select(ce =>
                new CompetenceEvaluationCommandParameter
                {
                    CompetenceId = ce.CompetenceId,
                    CompetenceComment = ce.CompetenceComment,
                    CriterionEvaluations = ce.CriterionEvaluations.Select(cre =>
                        new CriterionEvaluationCommandParameter
                        {
                            CriterionId = cre.CriterionId,
                            Score = cre.Score,
                            CriterionComment = cre.CriterionComment
                        }).ToList()
                }).ToList()
        };

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : result.Error!.ToProblemDetails();
    })
    .Produces<Guid>()
    .WithSummary("Отправить оценку");
//.RequireAuthorization("Authenticated"); //todo
#endregion

app.Run();