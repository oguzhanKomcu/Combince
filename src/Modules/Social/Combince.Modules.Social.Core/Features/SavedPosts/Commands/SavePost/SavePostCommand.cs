using Combince.Modules.Social.Core.Common;
using MediatR;
using FluentValidation;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Combince.Modules.Social.Core.Abstractions;
using Combince.Modules.Social.Core.Entities;

namespace Combince.Modules.Social.Core.Features.SavedPosts.Commands.SavePost;

public record SavePostCommand(Guid UserId, Guid PostId) : IRequest<Result<Guid>>;

public class SavePostCommandValidator : AbstractValidator<SavePostCommand>
{
    public SavePostCommandValidator(ILocalizedMessageProvider messageProvider)
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(messageProvider.GetMessage("ValidationMessages", "Social:FollowerIdRequired")); 

        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage(messageProvider.GetMessage("ValidationMessages", "Ratings:PostIdNotEmpty"));
    }
}

public class SavePostCommandHandler : IRequestHandler<SavePostCommand, Result<Guid>>
{
    private readonly ISocialDbContext _context;
    private readonly ILocalizedMessageProvider _messageProvider;

    public SavePostCommandHandler(ISocialDbContext context, ILocalizedMessageProvider messageProvider)
    {
        _context = context;
        _messageProvider = messageProvider;
    }

    public async Task<Result<Guid>> Handle(SavePostCommand request, CancellationToken cancellationToken)
    {
        var alreadySaved = await _context.SavedPosts
            .AnyAsync(x => x.UserId == request.UserId && x.PostId == request.PostId, cancellationToken);

        if (alreadySaved)
        {
            var alreadySavedMsg = _messageProvider.GetUserMessage("Social:AlreadySavedThisPost");
            return Result<Guid>.Failure(alreadySavedMsg, HttpStatusCode.BadRequest);
        }

        var savedPost = new SavedPost(request.UserId, request.PostId);

        _context.SavedPosts.Add(savedPost);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(savedPost.Id);
    }
}