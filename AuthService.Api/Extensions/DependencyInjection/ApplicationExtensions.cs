using System.Text.Json.Serialization;
using AuthService.Application.Behaviors;
using AuthService.Application.Commands;
using FluentValidation;

namespace AuthService.Api.Extensions.DependencyInjection;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<CreateRegistrationCommand>();
        
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(CreateRegistrationCommandHandler).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }
}