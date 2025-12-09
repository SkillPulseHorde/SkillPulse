using ReportService.Api.Extensions.ServiceRegistration;

namespace ReportService.Api.Extensions;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddServiceClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddUserServiceClient(configuration);
        services.AddAssessmentServiceClient(configuration);
        services.AddRecommendationServiceClient(configuration);

        return services;
    }
}