using AssessmentService.Api.Dto;
using AssessmentService.Application.Queries;
using MediatR;

namespace AssessmentService.Api.Endpoints.Competences;

public static class GetAllCompetences
{
    public static IEndpointRouteBuilder MapGetAllCompetencesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/competences", async (IMediator mediator, CancellationToken ct) =>
            {
                var query = new GetAllCompetencesQuery();

                var result = await mediator.Send(query, ct);

                return result.IsSuccess
                    ? Results.Ok(result.Value.Select(c => new CompetenceResponseDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Criteria = c.Criteria.Select(cr => new CriterionResponseDto
                        {
                            Id = cr.Id,
                            Name = cr.Name
                        }).ToList()
                    }).ToList())
                    : result.Error!.ToProblemDetails();
            })
            .Produces<List<CompetenceResponseDto>>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Получить все компетенции")
            .RequireAuthorization("Authenticated");

        return app;
    }
}

