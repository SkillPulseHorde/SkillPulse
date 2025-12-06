using AuthService.Api.Endpoints;

namespace AuthService.Api.Extensions;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapCreateRegistrationEndpoint();
        app.MapLoginEndpoint();
        app.MapLogoutEndpoint();
        app.MapRefreshTokensEndpoint();

        return app;
    }
}