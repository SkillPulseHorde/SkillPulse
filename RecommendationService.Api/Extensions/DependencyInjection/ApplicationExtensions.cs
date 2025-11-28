using System.Text.Json.Serialization;
using RecommendationService.Application.Commands;

namespace RecommendationService.Api.Extensions.DependencyInjection;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // FluentValidation
        //services.AddValidatorsFromAssemblyContaining<CreateRegistrationCommand>();

        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(GetRecommendationsByUserIdCommand).Assembly);
            //cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }
}