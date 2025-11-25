using AssessmentService.Api.Dto;
using AssessmentService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AssessmentService.Api.Endpoints.Competences;

public static class GetAllCompetences
{
    public static IEndpointRouteBuilder MapGetAllCompetencesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/assessments/competences/{evaluateeId:guid}", async (
                [FromRoute] Guid evaluateeId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var query = new GetAllCompetencesQuery(evaluateeId);

                var result = await mediator.Send(query, ct);

                return result.IsSuccess
                    ? Results.Ok(result.Value.Select(c => c.ToDto())
                        .ToList())
                    : result.Error!.ToProblemDetails();
            })
            .Produces<List<CompetenceResponseDto>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Получить все компетенции")
            .RequireAuthorization("Authenticated");

        return app;
    }
}

