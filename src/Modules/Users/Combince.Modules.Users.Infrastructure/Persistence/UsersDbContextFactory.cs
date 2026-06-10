using System.IO;
using Combince.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Combince.Modules.Users.Infrastructure.Persistence;

public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        string basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "Host", "Combince.Host.Api");

        if (!Directory.Exists(basePath))
        {
            basePath = Path.Combine(Directory.GetCurrentDirectory(), "src", "Host", "Combince.Host.Api");
        }

        string connectionString;

        if (Directory.Exists(basePath) && File.Exists(Path.Combine(basePath, "appsettings.json")))
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        else
        {
            connectionString = "Server=localhost,1433;Database=CombinceDb;User Id=sa;Password=CombincePassword123!;TrustServerCertificate=True;";
        }

        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new UsersDbContext(optionsBuilder.Options);
    }
}