using System.Text.Json.Serialization;
using FluentValidation;
using ReportService.Application;
using ReportService.Application.Behaviors;
using ReportService.Application.Commands;
using ReportService.Infrastructure;

namespace ReportService.Api.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(GenerateReportCommandHandler).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        
        // FluentValidation
        services.AddValidatorsFromAssemblyContaining<GenerateReportCommand>();
        
        services.AddScoped<IReportGenerator, ReportGenerator>();
        
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }
}