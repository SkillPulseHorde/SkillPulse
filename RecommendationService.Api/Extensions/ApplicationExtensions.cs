using System.Text.Json.Serialization;
using RecommendationService.Application.Commands;

namespace RecommendationService.Api.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(GetRecommendationsByAssessmentIdCommand).Assembly);
        });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }
}