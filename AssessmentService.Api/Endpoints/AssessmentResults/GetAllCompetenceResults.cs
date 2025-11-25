using AssessmentService.Api.Dto;
using AssessmentService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AssessmentService.Api.Endpoints.AssessmentResults;

public static class GetAllCompetenceResults
{
    public static IEndpointRouteBuilder MapGetAllCompetenceResultsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/assessments/users/{userId:guid}/competences/{competenceId:guid}/results", async (
                [FromRoute] Guid userId,
                [FromRoute] Guid competenceId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var query = new GetAllCompetenceResultsQuery(userId, competenceId);
                var result = await mediator.Send(query, ct);

                return result.IsSuccess
                    ? Results.Ok(result.Value.ToDto())
                    : result.Error!.ToProblemDetails();
            })
            .Produces<List<CompetenceResultHistoryDto>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Получить историю результатов по компетенции для пользователя")
            .RequireAuthorization("Authenticated");

        return app;
    }
}