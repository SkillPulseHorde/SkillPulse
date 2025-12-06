using Common.Shared.Auth.Extensions;

namespace RecommendationService.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationConfiguration(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddJwtAuthentication(options =>
        {
            options.SecretKey = configuration["JWT_SECRET_KEY"]
                                ?? configuration["Jwt:SecretKey"]
                                ?? throw new InvalidOperationException("JWT SecretKey не найден");
        });

        return services;
    }
}