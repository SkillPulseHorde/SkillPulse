using Common.Shared.Auth.Extensions;

namespace AssessmentService.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddJwtAuthentication(options =>
        {
            options.SecretKey = configuration["Jwt:SecretKey"]
                                ?? throw new InvalidOperationException("JWT SecretKey не найден");
        });

        return services;
    }
}