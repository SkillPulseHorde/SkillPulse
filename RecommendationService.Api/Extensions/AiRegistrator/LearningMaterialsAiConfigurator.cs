using RecommendationService.Infrastructure.AI.Configuration;

namespace RecommendationService.Api.Extensions.AiRegistrator;

public static class LearningMaterialsAiConfigurator
{
    public static IServiceCollection AddLearningMaterialsAiConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LearningMaterialsAiOptions>(options =>
        {
            options.ApiKey = configuration.GetSection("AI:LearningMaterials:ApiKey").Value
                             ?? throw new InvalidOperationException("AI LearningMaterials ApiKey не найден");
            options.BaseUrl = configuration.GetSection("AI:LearningMaterials:BaseUrl").Value
                              ?? throw new InvalidOperationException("AI LearningMaterials BaseUrl не найден");
            options.Model = configuration.GetSection("AI:LearningMaterials:Model").Value
                            ?? throw new InvalidOperationException("AI LearningMaterials Model не найден");
        });

        return services;
    }
}