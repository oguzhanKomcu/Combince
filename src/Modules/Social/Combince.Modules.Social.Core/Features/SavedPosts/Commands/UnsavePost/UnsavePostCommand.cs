using Combince.Modules.Social.Core.Common;
using MediatR;
using FluentValidation;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Combince.Modules.Social.Core.Abstractions;

namespace Combince.Modules.Social.Core.Features.SavedPosts.Commands.UnsavePost;

public record UnsavePostCommand(Guid UserId, Guid PostId) : IRequest<Result<Unit>>;

public class UnsavePostCommandValidator : AbstractValidator<UnsavePostCommand>
{
    public UnsavePostCommandValidator(ILocalizedMessageProvider messageProvider)
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PostId).NotEmpty();
    }
}

public class UnsavePostCommandHandler : IRequestHandler<UnsavePostCommand, Result<Unit>>
{
    private readonly ISocialDbContext _context;
    private readonly ILocalizedMessageProvider _messageProvider;

    public UnsavePostCommandHandler(ISocialDbContext context, ILocalizedMessageProvider messageProvider)
    {
        _context = context;
        _messageProvider = messageProvider;
    }

    public async Task<Result<Unit>> Handle(UnsavePostCommand request, CancellationToken cancellationToken)
    {
        var savedPost = await _context.SavedPosts
            .FirstOrDefaultAsync(x => x.UserId == request.UserId && x.PostId == request.PostId, cancellationToken);

        if (savedPost == null)
        {
            var notFoundMsg = _messageProvider.GetUserMessage("Social:SavedPostNotFound");
            return Result<Unit>.Failure(notFoundMsg, HttpStatusCode.NotFound);
        }

        _context.SavedPosts.Remove(savedPost);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}