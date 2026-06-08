using System;
using System.IO;
using Combince.Modules.Ratings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Combince.Modules.Ratings.Infrastructure.Persistence;

public class RatingsDbContextFactory : IDesignTimeDbContextFactory<RatingsDbContext>
{
    public RatingsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RatingsDbContext>();

        string currentDir = AppDomain.CurrentDomain.BaseDirectory;
        DirectoryInfo directory = new DirectoryInfo(currentDir);

        while (directory != null && !directory.Name.Equals("combince", StringComparison.OrdinalIgnoreCase))
        {
            directory = directory.Parent;
        }

        string basePath = directory != null
            ? Path.Combine(directory.FullName, "src", "Host", "Combince.Host.Api")
            : Directory.GetCurrentDirectory();

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        optionsBuilder.UseSqlServer(connectionString);

        return new RatingsDbContext(optionsBuilder.Options);
    }
}