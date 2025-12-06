using Microsoft.EntityFrameworkCore;
using UserService.Domain.Repos;
using UserService.Infrastructure.Db;
using UserService.Infrastructure.Repos;

namespace UserService.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // База данных
        services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("UserDb")));
        
        // Репозитории
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}