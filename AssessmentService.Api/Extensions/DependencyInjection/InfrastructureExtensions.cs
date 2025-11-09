using AssessmentService.Domain.Repos;
using AssessmentService.Infrastructure.Db;
using AssessmentService.Infrastructure.Repos;
using AssessmentService.Infrastructure.Http.ServiceClientOptions;
using AssessmentService.Api.Extensions.ServiceCollectionExtensions;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.Api.Extensions.DependencyInjection;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // База данных
        services.AddDbContext<AssessmentDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("AssessmentDb")));

        // Репозитории
        services.AddScoped<IAssessmentRepository, AssessmentRepository>();
        services.AddScoped<IEvaluationRepository, EvaluationRepository>();

        // Клиенты
        services.Configure<UserServiceOptions>(configuration);
        services.AddUserServiceClient(configuration);

        return services;
    }
}