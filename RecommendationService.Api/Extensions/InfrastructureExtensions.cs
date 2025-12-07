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
        services.AddRecommendationsAiConfiguration(configuration);
        services.AddLearningMaterialsAiConfiguration(configuration);


        // Сервисы
        //services.AddScoped<IAiRecommendationsGeneratorService, OpenAiApiRecommendationsGeneratorService>(); // Для любой нейросети, работающей по OpenApiAi
        services.AddScoped<IAiRecommendationsGeneratorService, GigaChatRecommendationsGeneratorService>(); // С получением AccessToken (ГигаЧат)

        services.AddScoped<IALearningMaterialsSearchService, OpenAiApiLearningMaterialGeneratorService>();
        // services.AddScoped<IALearningMaterialsSearchService, GigaChatLearningMaterialsGeneratorService>();


        // Клиенты
        services.Configure<UserServiceOptions>(configuration);
        services.AddUserServiceClient(configuration);

        services.Configure<AssessmentServiceOptions>(configuration);
        services.AddAssessmentServiceClient(configuration);

        return services;
    }
}