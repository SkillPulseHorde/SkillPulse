using AssessmentService.Api.Dto;
using AssessmentService.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AssessmentService.Api.Endpoints.Assessments;

public static class CreateAssessment
{
    public static IEndpointRouteBuilder MapCreateAssessmentEndpoint(this IEndpointRouteBuilder app)
    {
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
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithSummary("Запустить аттестацию")
            .RequireAuthorization("HROnly");

        return app;
    }
}