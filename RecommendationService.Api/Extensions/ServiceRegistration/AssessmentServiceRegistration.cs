using Microsoft.Extensions.Options;
using RecommendationService.Application.ServiceClientsAbstract;
using RecommendationService.Infrastructure.Http.ServiceClientOptions;
using RecommendationService.Infrastructure.Http.ServiceClients;

namespace RecommendationService.Api.Extensions.ServiceRegistration;

public static class AssessmentServiceRegistration
{
    public static IServiceCollection AddAssessmentServiceClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<AssessmentServiceOptions>()
            .Bind(configuration.GetSection("Services:AssessmentService"))
            .ValidateDataAnnotations()
            .Validate(options => Uri.IsWellFormedUriString(options.BaseUrl, UriKind.Absolute),
                "(AssessmentService) BaseUrl должен быть валидным URI")
            .Validate(options => !string.IsNullOrWhiteSpace(options.InternalToken),
                "(AssessmentService) InternalToken должен быть передан")
            .ValidateOnStart();

        services.AddHttpClient<IAssessmentServiceClient, AssessmentServiceClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<AssessmentServiceOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        return services;
    }
}