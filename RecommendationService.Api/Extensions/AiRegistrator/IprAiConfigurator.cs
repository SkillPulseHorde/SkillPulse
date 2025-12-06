using RecommendationService.Infrastructure.AI.Configuration;

namespace RecommendationService.Api.Extensions.AiRegistrator;

public static class IprAiConfigurator
{
    public static IServiceCollection AddIprAiConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<IprAiOptions>(options =>
        {
            options.ApiKey = configuration.GetSection("AI:IPR:ApiKey").Value
                             ?? throw new InvalidOperationException("AI IPR ApiKey не найден");
            options.BaseUrl = configuration.GetSection("AI:IPR:BaseUrl").Value
                              ?? throw new InvalidOperationException("AI IPR BaseUrl не найден");
            options.Model = configuration.GetSection("AI:IPR:Model").Value
                            ?? throw new InvalidOperationException("AI IPR Model не найден");
        });

        return services;
    }
}