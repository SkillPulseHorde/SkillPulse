using AssessmentService.Api.Dto;
using AssessmentService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AssessmentService.Api.Endpoints.Assessments;

public static class GetCompletedAssessmentsByEvaluateeId
{
    public static IEndpointRouteBuilder MapGetCompletedAssessmentsByEvaluateeIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/assessments/evaluatee/{userId:guid}/completed", async (
                [FromRoute] Guid userId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var query = new GetCompletedAssessmentsByEvaluateeIdQuery(userId);

                var result = await mediator.Send(query, ct);

                return result.IsSuccess
                    ? Results.Ok(result.Value.Select(a => a.ToShortInfoResponseDto()).ToList())
                    : result.Error!.ToProblemDetails();
            })
            .Produces<List<AssessmentShortInfoResponseDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Получить завершённые аттестации оцениваемого")
            .WithDescription("Возвращает Id и периоды завершённых аттестаций, оценивающих заданного пользователя.")
            .RequireAuthorization("Authenticated");

        return app;
    }
}
