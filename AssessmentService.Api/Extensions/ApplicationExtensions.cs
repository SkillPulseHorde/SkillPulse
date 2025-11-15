using System.Text.Json.Serialization;
using AssessmentService.Application.Behaviors;
using AssessmentService.Application.Commands;
using FluentValidation;

namespace AssessmentService.Api.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(CreateAssessmentCommandHandler).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        
        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<CreateAssessmentCommand>();
        
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }
}