using RecommendationService.Infrastructure.AI.Configuration;

namespace RecommendationService.Api.Extensions.AiRegistrator;

public static class LearningMaterialAiConfigurator
{
    public static IServiceCollection AddLearningMaterialAiConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LearningMaterialAiOptions>(options =>
        {
            options.ApiKey = configuration.GetSection("AI:LearningMaterial:ApiKey").Value
                             ?? throw new InvalidOperationException("AI LearningMaterial ApiKey не найден");
            options.BaseUrl = configuration.GetSection("AI:LearningMaterial:BaseUrl").Value
                              ?? throw new InvalidOperationException("AI LearningMaterial BaseUrl не найден");
            options.Model = configuration.GetSection("AI:LearningMaterial:Model").Value
                            ?? throw new InvalidOperationException("AI LearningMaterial Model не найден");
        });

        return services;
    }
}