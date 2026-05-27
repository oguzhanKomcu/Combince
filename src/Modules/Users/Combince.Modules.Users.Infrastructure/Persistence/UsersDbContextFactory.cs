using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Combince.Modules.Users.Infrastructure.Persistence;

public class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
{
    public UsersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UsersDbContext>();

        var connectionString = "Server=localhost,1433;Database=CombinceDb;User Id=sa;Password=CombincePassword123!;TrustServerCertificate=True;";

        optionsBuilder.UseSqlServer(connectionString);

        return new UsersDbContext(optionsBuilder.Options);
    }
}