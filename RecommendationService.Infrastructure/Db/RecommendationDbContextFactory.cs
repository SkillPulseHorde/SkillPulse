using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RecommendationService.Infrastructure.Db;

public class RecommendationDbContextFactory : IDesignTimeDbContextFactory<RecommendationDbContext>
{
    public RecommendationDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()  
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<RecommendationDbContext>();
        var connString = config.GetConnectionString("RecommendationDb");

        optionsBuilder.UseNpgsql(connString);

        return new RecommendationDbContext(optionsBuilder.Options);
    }
}