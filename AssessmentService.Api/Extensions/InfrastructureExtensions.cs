using AssessmentService.Api.Extensions.ServiceRegistration;
using AssessmentService.Domain.Repos;
using AssessmentService.Infrastructure.Db;
using AssessmentService.Infrastructure.Http.ServiceClientOptions;
using AssessmentService.Infrastructure.Repos;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.Api.Extensions;

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
        services.AddScoped<ICompetenceRepository, CompetenceRepository>();
        services.AddScoped<IUserEvaluatorRepository, UserEvaluatorRepository>();
        services.AddScoped<IAssessmentResultRepository, AssessmentResultRepository>();


        // Клиенты
        services.AddUserServiceClient(configuration);

        return services;
    }
}