using Combince.Modules.PostComments.Core.Abstractions;
using Combince.Modules.PostComments.Core.Features.Comments.Commands.CreateComment;
using Combince.Modules.PostComments.Core.Features.Comments.Commands.UpdateComment;
using Combince.Modules.PostComments.Core.Features.Comments.Commands.DeleteComment;
using Combince.Modules.PostComments.Core.Features.Comments.Queries.GetPostComments;
using Combince.Modules.PostComments.Core.Common;
using Combince.Modules.PostComments.Infrastructure.Persistence;
using Combince.Modules.PostComments.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using FluentValidation;

namespace Combince.Modules.PostComments.Infrastructure;

public static class PostCommentsModuleExtensions
{
    public static IServiceCollection AddPostCommentsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PostCommentsDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        });

        services.AddScoped<IPostCommentsDbContext>(provider => provider.GetRequiredService<PostCommentsDbContext>());

        services.AddSingleton<ILocalizedMessageProvider, JsonMessageProvider>();

        services.AddTransient<IRequestHandler<CreateCommentCommand, Result<CommentSuccessResponse>>, CreateCommentCommandHandler>();
        services.AddTransient<IRequestHandler<UpdateCommentCommand, Result<CommentUpdateSuccessResponse>>, UpdateCommentCommandHandler>();
        services.AddTransient<IRequestHandler<DeleteCommentCommand, Result<CommentDeleteSuccessResponse>>, DeleteCommentCommandHandler>();
        services.AddTransient<IRequestHandler<GetPostCommentsQuery, Result<List<CommentDto>>>, GetPostCommentsQueryHandler>();

        services.AddValidatorsFromAssembly(typeof(CreateCommentCommand).Assembly);

        return services;
    }
}