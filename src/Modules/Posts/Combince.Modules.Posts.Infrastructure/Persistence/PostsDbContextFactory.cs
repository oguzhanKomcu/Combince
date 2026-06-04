using Combince.Modules.Posts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Combince.Modules.Posts.Infrastructure;

public class PostsDbContextFactory : IDesignTimeDbContextFactory<PostsDbContext>
{
    public PostsDbContext CreateDbContext(string[] args)
    {
        string currentDir = Directory.GetCurrentDirectory();
        string basePath = currentDir;

        if (!basePath.EndsWith("Combince.Host.Api"))
        {
            basePath = Path.Combine(currentDir, "src", "Host", "Combince.Host.Api");

            if (!Directory.Exists(basePath))
            {
                basePath = Path.Combine(currentDir, "..", "Host", "Combince.Host.Api");
            }
        }

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<PostsDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new PostsDbContext(optionsBuilder.Options);
    }
}