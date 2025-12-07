using System.Text;
using Common.Shared.Auth.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Shared.Auth.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        Action<JwtAuthOptions> configureOptions)
    {
        services
            .AddOptions<JwtAuthOptions>()
            .Configure(configureOptions)
            .ValidateDataAnnotations()
            .Validate(options => Encoding.UTF8.GetByteCount(options.SecretKey) >= 32,
                "JWT SecretKey должен быть минимум 32 байта")
            .ValidateOnStart();

        var jwtOptions = new JwtAuthOptions();
        configureOptions(jwtOptions);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = jwtOptions.ValidateAudience,
                    ValidateLifetime = true,
                    ValidateIssuer = jwtOptions.ValidateIssuer,
                    ClockSkew = jwtOptions.ClockSkew,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                };
            });

        return services;
    }
}