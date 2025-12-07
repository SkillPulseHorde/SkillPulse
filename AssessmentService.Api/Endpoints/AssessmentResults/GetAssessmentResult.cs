using AssessmentService.Api.Dto;
using AssessmentService.Api.Middleware;
using AssessmentService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AssessmentService.Api.Endpoints.AssessmentResults;

public static class GetAssessmentResult
{
    public static IEndpointRouteBuilder MapGetAssessmentResultEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/assessments/{assessmentId:guid}/result", async (
                [FromRoute] Guid assessmentId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var query = new GetAssessmentResultQuery(assessmentId);
                var result = await mediator.Send(query, ct);

                return result.IsSuccess
                    ? Results.Ok(result.Value.ToDto())
                    : result.Error!.ToProblemDetails();
            })
            .Produces<AssessmentResultResponseDto>()
            .AddEndpointFilter<RequireInternalRoleFilter>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Получить результат аттестации по её ID")
            .RequireAuthorization("AuthenticatedAndService");

        return app;
    }
}
