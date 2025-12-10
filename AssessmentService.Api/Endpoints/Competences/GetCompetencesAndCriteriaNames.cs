using AssessmentService.Api.Dto;
using AssessmentService.Application.Queries;
using MediatR;

namespace AssessmentService.Api.Endpoints.Competences;

public static class GetCompetencesAndCriteriaNames
{
    public static IEndpointRouteBuilder MapGetCompetencesAndCriteriaNamesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/assessments/competences-and-criteria-names/", async (
                IMediator mediator,
                CancellationToken ct) =>
            {
                var query = new GetCompetencesAndCriteriaNamesQuery();
                var result = await mediator.Send(query, ct);

                return result.IsSuccess
                    ? Results.Ok(result.Value.ToDto())
                    : result.Error!.ToProblemDetails();
            })
            .Produces<CompetencesAndCriteriaNamesModelResponseDto>()
            .RequireAuthorization("ServiceOnly")
            .WithSummary("Получить словари (ID -> название компетенции/критерия)");

        return app;
    }
}
