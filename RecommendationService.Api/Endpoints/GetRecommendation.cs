using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using RecommendationService.Api.Dto;
using RecommendationService.Application.Commands;

namespace RecommendationService.Api.Endpoints;

public static class GetRecommendation
{
    public static IEndpointRouteBuilder MapGetRecommendationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/recommendation", async (
                [FromBody] GetRecommendationRequestDto request,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var command = new GetRecommendationsByUserIdCommand
                {
                    UserId = request.UserId,
                    AssessmentId = request.AssessmentId
                };
                var result = await mediator.Send(command, ct);

                return result.IsSuccess
                    ? Results.Ok(result.Value.ToResponseDto())
                    : result.Error!.ToProblemDetails();
            })
            .Produces<string>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Получить рекомендацию для конкретного пользователя")
            .WithDescription("Получить рекомендацию для пользователя по идентификатору пользователя и тестирования")
            .WithOpenApi(operation =>
            {
                operation.RequestBody.Content["application/json"].Example = new OpenApiObject
                {
                    ["UserId"] = new OpenApiString("11111111-1111-1111-1111-111111111002"),
                    ["AssessmentId"] = new OpenApiString("e7c706ce-99f4-4b2b-a15b-d3cead4d4366")
                };
                return operation;
            })
            .RequireAuthorization("Authenticated");

        return app;
    }
}