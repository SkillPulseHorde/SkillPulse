using Microsoft.EntityFrameworkCore;
using UserService.Infrastructure.Db;

namespace UserService.Extensions.DependencyInjection;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("UserDb")));
        
        return services;
    }
}