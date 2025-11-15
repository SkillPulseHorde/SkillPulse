using AssessmentService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AssessmentService.Api.Endpoints.Assessments;

public static class GetEvaluatorsByUserId
{
    public static IEndpointRouteBuilder MapGetEvaluatorsByUserIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/assessments/evaluators/{userId:guid}", async (
                [FromRoute] Guid userId,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var query = new GetEvaluatorIdsByUserIdQuery(userId);

                var result = await mediator.Send(query, ct);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.Error!.ToProblemDetails();
            })
            .Produces<List<Guid>>()
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Получить Id рецензентов пользователя по его ID")
            .RequireAuthorization("Authenticated");

        return app;
    }
}

