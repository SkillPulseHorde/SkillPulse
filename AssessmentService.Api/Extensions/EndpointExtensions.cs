using AssessmentService.Api.Endpoints.Assessments;
using AssessmentService.Api.Endpoints.Competences;
using AssessmentService.Api.Endpoints.Evaluations;

namespace AssessmentService.Api.Extensions;

public static class EndpointExtensions
{
    private static IEndpointRouteBuilder MapAssessmentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateAssessmentEndpoint();
        app.MapGetAssessmentByIdEndpoint();
        app.MapUpdateAssessmentEndpoint();
        app.MapDeleteAssessmentEndpoint();
        app.MapGetEvaluatorsByUserIdEndpoint();
        app.MapUpdateEvaluatorsForUserEndpoint();
        app.MapGetAssessmentsEndpoint();
        app.MapGetActiveAssessmentsByEvaluatorIdEndpoint();

        return app;
    }

    private static IEndpointRouteBuilder MapCompetenceEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetAllCompetencesEndpoint();

        return app;
    }

    private static IEndpointRouteBuilder MapEvaluationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateEvaluationEndpoint();

        return app;
    }

    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapAssessmentEndpoints();
        app.MapCompetenceEndpoints();
        app.MapEvaluationEndpoints();

        return app;
    }
}

