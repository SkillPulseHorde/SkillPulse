using Microsoft.EntityFrameworkCore;
using RecommendationService.Application.AI;
using RecommendationService.Domain.Repos;
using RecommendationService.Infrastructure.AI.Configuration;
using RecommendationService.Infrastructure.AI.Services;
using RecommendationService.Infrastructure.Db;
using RecommendationService.Infrastructure.Repos;

namespace RecommendationService.Api.Extensions.DependencyInjection;

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

        // AI конфигуратор
        services.Configure<AiOptions>(options =>
        {
            options.ApiKey = configuration.GetSection("AI:ApiKey").Value 
                             ?? throw new InvalidOperationException("Ai ApiKey не найден");
            options.BaseUrl = configuration.GetSection("AI:BaseUrl").Value 
                             ?? throw new InvalidOperationException("Ai BaseUrl не найден");
            options.Model = configuration.GetSection("AI:Model").Value 
                             ?? throw new InvalidOperationException("Ai Model не найден");
        });

        // Сервисы
        services.AddScoped<IAiPlanGeneratorService, OpenAiApiPlanGeneratorService>(); // Для любой нейросети, работающей по OpenApiAi
        services.AddScoped<IAiPlanGeneratorService, GigaChatPlanGeneratorService>(); // С получением AccessToken (ГигаЧат)
        
        // services.AddScoped<IAIMaterialSearchService, >(); // TODO Добавить сервис для генерации Ссылок

        return services;
    }
}