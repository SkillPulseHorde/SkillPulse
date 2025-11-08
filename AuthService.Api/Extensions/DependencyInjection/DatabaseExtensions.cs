using AuthService.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Api.Extensions.DependencyInjection;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection  services,
        IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("AuthDb")));

        return services;
    }
}