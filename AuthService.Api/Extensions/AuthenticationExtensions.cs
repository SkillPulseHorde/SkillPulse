using System.Text;
using AuthService.Infrastructure;
using Common.Shared.Auth.Extensions;

namespace AuthService.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(option =>
        {
            option.SecretKey = configuration.GetSection("Jwt:SecretKey").Value
                               ?? throw new InvalidOperationException("JWT_SECRET_KEY не найден");
            if (Encoding.UTF8.GetByteCount(option.SecretKey) < 32)
                throw new InvalidOperationException("JWT_SECRET_KEY должен быть не менее 32 символа");

            option.AccessExpiresMinutes = int.TryParse(configuration.GetSection("Jwt:AccessExpiresMinutes").Value, out var accMinutes)
                ? accMinutes
                : throw new InvalidOperationException("JWT_ACCESS_EXPIRES_MINUTES не найден");

            option.RefreshExpiresHours = int.TryParse(configuration.GetSection("Jwt:RefreshExpiresHours").Value, out var refHours)
                ? refHours
                : throw new InvalidOperationException("JWT_REFRESH_EXPIRES_HOURS не найден");
        });

        services.AddJwtAuthentication(options =>
        {
            options.SecretKey = configuration.GetSection("Jwt:SecretKey").Value ?? "";
        });

        return services;
    }
}