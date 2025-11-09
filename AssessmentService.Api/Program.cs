using System.ComponentModel.DataAnnotations;
using AssessmentService.Api;
using AssessmentService.Api.Dto;
using AssessmentService.Application.Commands;
using AssessmentService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Common.Shared.Auth.Extensions;
using AssessmentService.Api.Extensions.DependencyInjection;
using AssessmentService.Api.Middleware;

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