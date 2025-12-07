using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AssessmentService.Infrastructure.Db
{
    public class AssessmentDbContextFactory : IDesignTimeDbContextFactory<AssessmentDbContext>
    {
        public AssessmentDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AssessmentDbContext>();
            var connString = config.GetConnectionString("AssessmentDb");

            optionsBuilder.UseNpgsql(connString);

            return new AssessmentDbContext(optionsBuilder.Options);
        }
    }
}