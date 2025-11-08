using AssessmentService.Domain.Repos;
using AssessmentService.Infrastructure.Repos;
using AssessmentService.Infrastructure.Http.ServiceClientOptions;
using AssessmentService.Api.Extensions.ServiceCollectionExtensions;

namespace AssessmentService.Api.Extensions.DependencyInjection;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Репозитории
        services.AddScoped<IAssessmentRepository, AssessmentRepository>();
        services.AddScoped<IEvaluationRepository, EvaluationRepository>();

        // Клиенты
        services.Configure<UserServiceOptions>(configuration);
        services.AddUserServiceClient(configuration);

        return services;
    }
}