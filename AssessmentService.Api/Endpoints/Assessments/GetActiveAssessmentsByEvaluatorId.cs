using AssessmentService.Api.Dto;
using AssessmentService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AssessmentService.Api.Endpoints.Assessments;

public static class GetActiveAssessmentsByEvaluatorId
{
    public static IEndpointRouteBuilder MapGetActiveAssessmentsByEvaluatorIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/assessments/evaluator/{userId:guid}/active", async (
                [FromRoute] Guid userId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var query = new GetActiveAssessmentsByEvaluatorIdQuery(userId);

                var result = await mediator.Send(query, ct);

                return result.IsSuccess
                    ? Results.Ok(result.Value.Select(a => a.ToResponseDto()).ToList())
                    : result.Error!.ToProblemDetails();
            })
            .Produces<List<AssessmentResponseDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Получить активные аттестации, назначенные рецензенту")
            .RequireAuthorization("Authenticated");

        return app;
    }
}

