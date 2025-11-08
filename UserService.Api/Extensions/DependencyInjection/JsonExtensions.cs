using System.Text.Json.Serialization;

namespace UserService.Extensions.DependencyInjection;

public static class JsonExtensions
{
    public static IServiceCollection AddJsonConfiguration(
        this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(option =>
        {
            option.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }
}