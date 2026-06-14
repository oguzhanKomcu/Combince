using Combince.Modules.Social.Core.Abstractions;
using Combince.Modules.Social.Core.Features.Follows.Commands.FollowUser;
using Combince.Modules.Social.Core.Features.Follows.Commands.UnfollowUser;
// 🔥 Yeni eklediğimiz Save/Unsave ve Get klasörlerinin namespace tanımları
using Combince.Modules.Social.Core.Features.SavedPosts.Commands.SavePost;
using Combince.Modules.Social.Core.Features.SavedPosts.Commands.UnsavePost;
using Combince.Modules.Social.Core.Features.SavedPosts.Queries.GetSavedPosts;
using Combince.Modules.Social.Infrastructure.Persistence;
using Combince.Modules.Social.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

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

            cfg.RegisterServicesFromAssembly(typeof(SavePostCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(UnsavePostCommand).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(GetSavedPostsQuery).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(FollowUserCommand).Assembly);
        services.AddValidatorsFromAssembly(typeof(UnfollowUserCommand).Assembly);

        services.AddValidatorsFromAssembly(typeof(SavePostCommand).Assembly);
        services.AddValidatorsFromAssembly(typeof(UnsavePostCommand).Assembly);

        return services;
    }
}