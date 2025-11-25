using AssessmentService.Api.Dto;
using AssessmentService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AssessmentService.Api.Endpoints.Assessments;

public static class GetAssessmentById
{
    public static IEndpointRouteBuilder MapGetAssessmentByIdEndpoint(this IEndpointRouteBuilder app)
    {
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
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Получить аттестацию по ID")
            .RequireAuthorization("Authenticated");

        return app;
    }
}

