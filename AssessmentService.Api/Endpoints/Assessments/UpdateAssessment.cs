using AssessmentService.Api.Dto;
using AssessmentService.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AssessmentService.Api.Endpoints.Assessments;

public static class UpdateAssessment
{
    public static IEndpointRouteBuilder MapUpdateAssessmentEndpoint(this IEndpointRouteBuilder app)
    {
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
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithSummary("Обновить аттестацию")
            .RequireAuthorization("HROnly");

        return app;
    }
}

