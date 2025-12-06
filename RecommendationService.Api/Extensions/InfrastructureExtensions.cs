using Microsoft.EntityFrameworkCore;
using RecommendationService.Api.Extensions.AiRegistrator;
using RecommendationService.Domain.Repos;
using RecommendationService.Infrastructure.AI.Services;
using RecommendationService.Infrastructure.Db;
using RecommendationService.Infrastructure.Repos;
using RecommendationService.Infrastructure.Http.ServiceClientOptions;
using RecommendationService.Api.Extensions.ServiceRegistration;
using RecommendationService.Application.AiServiceAbstract;

namespace RecommendationService.Api.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // База данных
        services.AddDbContext<RecommendationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("RecommendationDb")));


        // Репозитории
        services.AddScoped<ILearningMaterialRepository, LearningMaterialRepository>();
        services.AddScoped<IIndividualDevelopmentPlanRepository, IndividualDevelopmentPlanRepository>();
        services.AddScoped<IThresholdValueRepository, ThresholdValueRepository>();


        // AI конфигураторы
        services.AddIprAiConfiguration(configuration);
        services.AddLearningMaterialAiConfiguration(configuration);


        // Сервисы
        services.AddScoped<IAiIprGeneratorService, GetPrompt>(); // Для любой нейросети, работающей по OpenApiAi
        // services.AddScoped<IAiPlanGeneratorService, GigaChatPlanGeneratorService>(); // С получением AccessToken (ГигаЧат)

        services.AddScoped<IAiLearningMaterialSearchService, OpenAiApiLearningMaterialGeneratorService>();
        // services.AddScoped<IAiMaterialSearchService, GigaChatLearningMaterialGeneratorService>();


        // Клиенты
        services.Configure<UserServiceOptions>(configuration);
        services.AddUserServiceClient(configuration);

        services.Configure<AssessmentServiceOptions>(configuration);
        services.AddAssessmentServiceClient(configuration);

        return services;
    }
}