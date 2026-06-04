using Combince.Modules.Posts.Core.Abstractions;
using Combince.Modules.Posts.Core.Features.Posts.Commands;
using Combince.Modules.Posts.Infrastructure.Persistence;
using Combince.Modules.Posts.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Combince.Modules.Posts.Infrastructure;

public static class PostsModuleExtensions
{
    public static IServiceCollection AddPostsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PostsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPostsDbContext>(provider => provider.GetRequiredService<PostsDbContext>());

        services.AddHttpContextAccessor();
        services.AddScoped<IMediaService, LocalMediaService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumers(typeof(PostsModuleExtensions).Assembly);
        });

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreatePostCommand).Assembly);
        });

        return services;
    }
}