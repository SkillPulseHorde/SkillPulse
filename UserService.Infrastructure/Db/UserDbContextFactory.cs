using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace UserService.Infrastructure.Db
{
    public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
    {
        public UserDbContext CreateDbContext(string[] args)
        {
            // Читаем конфиг
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()  
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
            var connString = config.GetConnectionString("UserDb");

            optionsBuilder.UseNpgsql(connString);

            return new UserDbContext(optionsBuilder.Options);
        }
    }
}