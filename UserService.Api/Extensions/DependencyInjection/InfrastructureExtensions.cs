using UserService.Domain.Repos;
using UserService.Infrastructure.Repos;

namespace UserService.Extensions.DependencyInjection;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Репозитории
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}