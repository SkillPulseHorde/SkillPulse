using System.Text.Json.Serialization;
using FluentValidation;
using UserService.Application.Behaviors;
using UserService.Application.Queries;

namespace UserService.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(GetUserByIdQueryHandler).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<GetUserByIdQuery>();

        // JsonOptions
        services.ConfigureHttpJsonOptions(option =>
        {
            option.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }
}