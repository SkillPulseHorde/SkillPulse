using AssessmentService.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace AssessmentService.Api.Extensions.DependencyInjection;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AssessmentDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("AssessmentDb")));

        return services;
    }
}