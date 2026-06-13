
using Combince.Modules.Social.Core.Abstractions;
using Combince.Modules.Social.Core.Features.Follows.Commands.FollowUser;
using Combince.Modules.Social.Core.Features.Follows.Commands.UnfollowUser;
using Combince.Modules.Social.Infrastructure.Persistence;
using Combince.Modules.Social.Infrastructure.Services;
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
        services.AddSingleton<ILocalizedMessageProvider, JsonMessageProvider>();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(FollowUserCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(UnfollowUserCommand).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(FollowUserCommand).Assembly);
        services.AddValidatorsFromAssembly(typeof(UnfollowUserCommand).Assembly);

        return services;
    }
}