using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Combince.Modules.PostComments.Infrastructure.Persistence;

public class PostCommentsDbContextFactory : IDesignTimeDbContextFactory<PostCommentsDbContext>
{
    public PostCommentsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PostCommentsDbContext>();

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

        return new PostCommentsDbContext(optionsBuilder.Options);
    }
}