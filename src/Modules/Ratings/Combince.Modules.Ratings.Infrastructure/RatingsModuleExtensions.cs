using Combince.Modules.Ratings.Core.Abstractions;
using Combince.Modules.Ratings.Core.Common;
using Combince.Modules.Ratings.Core.Features.Ratings.Commands.RatePost;
using Combince.Modules.Ratings.Infrastructure.Persistence;
using Combince.Modules.Ratings.Infrastructure.Services;
using FluentValidation; 
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Combince.Modules.Ratings.Infrastructure;

public static class RatingsModuleExtensions
{
    public static IServiceCollection AddRatingsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<RatingsDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });
        services.AddSingleton<ILocalizedMessageProvider, JsonMessageProvider>();
        services.AddScoped<IRatingsDbContext>(provider => provider.GetRequiredService<RatingsDbContext>());
        services.AddTransient<IRequestHandler<RatePostCommand, Result<RatingSuccessResponse>>, RatePostCommandHandler>();
        services.AddValidatorsFromAssembly(typeof(RatePostCommand).Assembly);

        return services;
    }
}