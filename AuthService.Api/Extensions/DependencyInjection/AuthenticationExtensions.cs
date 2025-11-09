using AuthService.Infrastructure;
using System.Text;
using Common.Shared.Auth.Extensions;

namespace AuthService.Api.Extensions.DependencyInjection;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthenticationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(option =>
        {
            option.SecretKey = configuration["JWT_SECRET_KEY"]
                               ?? throw new InvalidOperationException("JWT_SECRET_KEY не найден");
            if (Encoding.UTF8.GetByteCount(option.SecretKey) < 32)
                throw new InvalidOperationException("JWT_SECRET_KEY должен быть не менее 32 символа");

            option.AccessExpiresMinutes = int.TryParse(configuration["JWT_ACCESS_EXPIRES_MINUTES"], out var accMinutes)
                ? accMinutes
                : throw new InvalidOperationException("JWT_ACCESS_EXPIRES_MINUTES не найден");

            option.RefreshExpiresHours = int.TryParse(configuration["JWT_REFRESH_EXPIRES_HOURS"], out var refHours)
                ? refHours
                : throw new InvalidOperationException("JWT_REFRESH_EXPIRES_HOURS не найден");
        });

        services.AddJwtAuthentication(options =>
        {
            options.SecretKey = configuration["JWT_SECRET_KEY"] ?? "";
        });

        return services;
    }
}