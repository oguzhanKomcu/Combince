using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Combince.Modules.Posts.Core.Abstractions;
using Combince.Modules.Posts.Core.Common;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.Posts.Core.Features.Posts.Commands.UpdatePost;

public record UpdatePostRequest(string Description);


public record UpdatePostCommand(Guid PostId, Guid UserId, string Description) : IRequest<Result<Unit>>;

public class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostCommandValidator(ILocalizedMessageProvider messageProvider)
    {
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage(messageProvider.GetMessage("ValidationMessages", "Posts:PostDescriptionNotEmpty"));
    }
}

public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, Result<Unit>>
{
    private readonly IPostsDbContext _context;
    private readonly ILocalizedMessageProvider _messageProvider;

    public UpdatePostCommandHandler(
        IPostsDbContext context,
        ILocalizedMessageProvider messageProvider)
    {
        _context = context;
        _messageProvider = messageProvider;
    }

    public async Task<Result<Unit>> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == request.PostId, cancellationToken);

        if (post == null)
        {
            var postNotFoundMsg = _messageProvider.GetUserMessage("Posts:PostNotFound");
            return Result<Unit>.Failure(postNotFoundMsg, HttpStatusCode.NotFound);
        }

        if (post.UserId != request.UserId)
        {
            var unauthorizedMsg = _messageProvider.GetUserMessage("Posts:UnauthorizedPostAction");
            return Result<Unit>.Failure(unauthorizedMsg, HttpStatusCode.Forbidden);
        }

        post.UpdateDescription(request.Description);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}