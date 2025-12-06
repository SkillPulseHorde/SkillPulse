using AuthService.Api.Extensions.ServiceRegistration;
using AuthService.Domain.Repos;
using AuthService.Application.interfaces;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Db;
using AuthService.Infrastructure.Repos;
using AuthService.Infrastructure.Http.ServiceClientOptions;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Api.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // База данных
        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("AuthDb")));
        
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