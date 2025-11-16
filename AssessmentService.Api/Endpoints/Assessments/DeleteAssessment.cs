using AssessmentService.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AssessmentService.Api.Endpoints.Assessments;

public static class DeleteAssessment
{
    public static IEndpointRouteBuilder MapDeleteAssessmentEndpoint(this IEndpointRouteBuilder app)
    {
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
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithSummary("Удалить аттестацию")
            .RequireAuthorization("HROnly");

        return app;
    }
}

