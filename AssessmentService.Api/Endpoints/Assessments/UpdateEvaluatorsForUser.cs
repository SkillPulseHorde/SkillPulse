using AssessmentService.Api.Dto;
using AssessmentService.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AssessmentService.Api.Endpoints.Assessments;

public static class UpdateEvaluatorsForUser
{
    public static IEndpointRouteBuilder MapUpdateEvaluatorsForUserEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/assessments/evaluators/{userId:guid}", async (
                [FromRoute] Guid userId,
                [FromBody] UpdateEvaluatorsRequestDto request,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var command = new UpdateEvaluatorsForUserCommand
                {
                    UserId = userId,
                    EvaluatorIds = request.EvaluatorIds ?? []
                };

                var result = await mediator.Send(command, ct);

                return result.IsSuccess
                    ? Results.NoContent()
                    : result.Error!.ToProblemDetails();
            })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .WithSummary("Обновить список рецензентов пользователя (полная замена)")
            .RequireAuthorization("Authenticated");

        return app;
    }
}

