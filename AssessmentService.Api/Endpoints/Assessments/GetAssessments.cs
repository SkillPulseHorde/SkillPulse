using System.ComponentModel.DataAnnotations;
using AssessmentService.Api.Dto;
using AssessmentService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AssessmentService.Api.Endpoints.Assessments;

public static class GetAssessments
{
    public static IEndpointRouteBuilder MapGetAssessmentsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/assessments", async (
                [Required] [FromQuery] bool isActive,
                IMediator mediator,
                CancellationToken ct) =>
            {
                var query = new GetAssessmentsQuery(isActive);

                var result = await mediator.Send(query, ct);

                return result.IsSuccess
                    ? Results.Ok(result.Value.Select(a => a.ToResponseDto()).ToList())
                    : result.Error!.ToProblemDetails();
            })
            .Produces<List<AssessmentResponseDto>>()
            .WithSummary("Получить список аттестаций")
            .WithOpenApi(operation =>
            {
                operation.Parameters[0].Description = "true - активные аттестации, false - предстоящие аттестации";
                return operation;
            })
            .ProducesProblem(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization("HROnly");

        return app;
    }
}

