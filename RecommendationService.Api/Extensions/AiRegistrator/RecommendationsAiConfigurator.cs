using RecommendationService.Infrastructure.AI.Configuration;

namespace RecommendationService.Api.Extensions.AiRegistrator;

public static class RecommendationsAiConfigurator
{
    public static IServiceCollection AddRecommendationsAiConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RecommendationsAiOptions>(options =>
        {
            options.ApiKey = configuration.GetSection("AI:Recommendations:ApiKey").Value
                             ?? throw new InvalidOperationException("AI Recommendations ApiKey не найден");
            options.BaseUrl = configuration.GetSection("AI:Recommendations:BaseUrl").Value
                              ?? throw new InvalidOperationException("AI Recommendations BaseUrl не найден");
            options.Model = configuration.GetSection("AI:Recommendations:Model").Value
                            ?? throw new InvalidOperationException("AI Recommendations Model не найден");
        });

        return services;
    }
}