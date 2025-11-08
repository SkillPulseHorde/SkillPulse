using Common.Shared.Auth.Extensions;

namespace UserService.Extensions.DependencyInjection;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddJwtAuthentication(options =>
        {
            options.SecretKey = configuration["Jwt:SecretKey"]
                                ?? configuration["JWT_SECRET_KEY"]
                                ?? throw new InvalidOperationException("JWT SecretKey не найден");
        });
        
        return services;
    }
}