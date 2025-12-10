using Microsoft.Extensions.Options;
using ReportService.Application.ServiceClientsAbstract;
using ReportService.Infrastructure.Http.ServiceClients.RecommendationServiceClient;

namespace ReportService.Api.Extensions.ServiceRegistration;

public static class RecommendationServiceRegistration
{
    public static IServiceCollection AddRecommendationServiceClient(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<RecommendationServiceOptions>()
            .Bind(configuration.GetSection("Services:RecommendationService"))
            .ValidateDataAnnotations()
            .Validate(options => Uri.IsWellFormedUriString(options.BaseUrl, UriKind.Absolute),
                "BaseUrl должен быть валидным URI")
            .Validate(options => !string.IsNullOrWhiteSpace(options.InternalToken),
                "InternalToken должен быть передан")
            .ValidateOnStart();

        services.AddHttpClient<IRecommendationServiceClient, RecommendationServiceClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<RecommendationServiceOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        return services;
    }
}