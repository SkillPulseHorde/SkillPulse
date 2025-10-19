using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AuthService.Infrastructure.Db
{
    public class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
    {
        public AuthDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()  
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
            var connString = config.GetConnectionString("UserDb");

            optionsBuilder.UseNpgsql(connString);

            return new AuthDbContext(optionsBuilder.Options);
        }
    }
}