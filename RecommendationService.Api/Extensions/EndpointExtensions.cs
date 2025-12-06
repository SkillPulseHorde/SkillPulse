using RecommendationService.Api.Endpoints;

namespace RecommendationService.Api.Extensions;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGetRecommendationEndpoint();

        return app;
    }
}
