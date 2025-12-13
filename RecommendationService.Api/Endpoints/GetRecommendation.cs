using System.Security.Claims;
using Common.Shared.Auth;
using Common.Shared.Auth.Models;
using Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using RecommendationService.Api.Dto;
using RecommendationService.Application.Commands;
using RecommendationService.Application.Queries;

namespace RecommendationService.Api.Endpoints;

public static class GetRecommendation
{
    public static IEndpointRouteBuilder MapGetRecommendationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/recommendations", async (
                [FromBody] GetRecommendationRequestDto request,
                IMediator mediator,
                ClaimsPrincipal user,
                CancellationToken ct) =>
            {
                var userInfo = await mediator.Send(new GetUserInfoForCheckAccessQuery(request.UserId), ct);
                if (!userInfo.IsSuccess)
                    return userInfo.Error!.ToProblemDetails();

                var profile = userInfo.Value;
                var canUserAccess = AccessPolicy.CanAccess(
                    user,
                    new TargetInfo(
                        Id: profile.Id,
                        Team: profile.TeamName,
                        ManagerId: profile.ManagerId,
                        Role: profile.Position));

                if (!canUserAccess)
                    return Error.Forbidden("Отсутствует доступ").ToProblemDetails();

                var command = new GetRecommendationsByAssessmentIdCommand
                {
                    UserId = request.UserId,
                    AssessmentId = request.AssessmentId
                };
                var result = await mediator.Send(command, ct);

                return result.IsSuccess
                    ? Results.Ok(result.Value.ToResponseDto())
                    : result.Error!.ToProblemDetails();
            })
            .Produces<RecommendationsResponseDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Получить рекомендацию для конкретного пользователя")
            .WithDescription("Получить рекомендацию для пользователя по идентификатору пользователя и аттестации")
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