using Combince.Modules.Users.Core.Abstractions;
using Combince.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Combince.Modules.Users.Infrastructure;

public static class UsersModuleExtensions
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<UsersDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IUsersDbContext>(provider => provider.GetRequiredService<UsersDbContext>());

        return services;
    }
}