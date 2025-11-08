using AuthService.Api.Extensions.ServiceCollectionExtensions;
using AuthService.Domain.Repos;
using AuthService.Application.interfaces;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Repos;
using AuthService.Infrastructure.Http.ServiceClientOptions;

namespace AuthService.Api.Extensions.DependencyInjection;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Репозитории
        services.AddScoped<IAuthRepository, AuthRepository>();

        // Сервисы
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();

        // Клиенты
        services.Configure<UserServiceOptions>(configuration);
        services.AddUserServiceClient(configuration);

        return services;
    }
}