using Combince.Modules.Social.Core.Abstractions;
using Combince.Modules.Social.Core.Features.Follows.Commands.FollowUser;
using Combince.Modules.Social.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Combince.Modules.Social.Infrastructure;

public static class SocialModuleExtensions
{
    public static IServiceCollection AddSocialModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SocialDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ISocialDbContext>(provider => provider.GetRequiredService<SocialDbContext>());

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(FollowUserCommand).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(FollowUserCommand).Assembly);

        return services;
    }
}