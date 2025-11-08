using System.Text.Json.Serialization;

namespace AuthService.Api.Extensions.DependencyInjection;

public static class JsonExtensions
{
    public static IServiceCollection AddJsonConfiguration(
        this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }
}