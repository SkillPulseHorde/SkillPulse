using System.Text.Json.Serialization;
using UserService.Application.Queries;
using UserService.Application.Behaviors;
using FluentValidation;

namespace UserService.Extensions.DependencyInjection;

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