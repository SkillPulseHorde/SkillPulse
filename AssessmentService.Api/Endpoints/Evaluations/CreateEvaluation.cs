using AssessmentService.Api.Dto.Evaluation;
using AssessmentService.Application.Commands;
using AssessmentService.Application.Commands.CommandParameters;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AssessmentService.Api.Endpoints.Evaluations;

public static class CreateEvaluation
{
    public static IEndpointRouteBuilder MapCreateEvaluationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/evaluations", async (
                [FromBody] CreateEvaluationRequestDto request,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var command = new CreateEvaluationCommand
                {
                    AssessmentId = request.AssessmentId,
                    EvaluatorId = request.EvaluatorId,
                    CompetenceEvaluations = request.CompetenceEvaluations.Select(ce =>
                        new CompetenceEvaluationCommandParameter
                        {
                            CompetenceId = ce.CompetenceId,
                            CompetenceComment = ce.CompetenceComment,
                            CriterionEvaluations = ce.CriterionEvaluations.Select(cre =>
                                new CriterionEvaluationCommandParameter
                                {
                                    CriterionId = cre.CriterionId,
                                    Score = cre.Score,
                                    CriterionComment = cre.CriterionComment
                                }).ToList()
                        }).ToList()
                };

                var result = await mediator.Send(command, ct);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.Error!.ToProblemDetails();
            })
            .Produces<Guid>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithSummary("Отправить оценку")
            .RequireAuthorization("Authenticated");

        return app;
    }
}
