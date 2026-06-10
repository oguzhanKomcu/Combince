using Combince.Modules.Social.Core.Abstractions;
using Combince.Modules.Social.Core.Common;
using Combince.Shared.Core.Abstractions;
using Combince.Shared.Core.Events.Modules;
using Combince.Shared.Core.Events.Modules.Follow;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Combince.Modules.Social.Core.Features.Follows.Commands.UnfollowUser;

public record UnfollowUserCommand(Guid FollowerId, Guid FollowingId) : IRequest<Result<UnfollowUserResponse>>;

public record UnfollowUserResponse(bool IsSuccess);

public class UnfollowUserCommandValidator : AbstractValidator<UnfollowUserCommand>
{
    public UnfollowUserCommandValidator(ILocalizedMessageProvider messageProvider)
    {
        RuleFor(x => x.FollowerId)
            .NotEmpty()
            .WithMessage(messageProvider.GetMessage("ValidationMessages", "Social:FollowerIdRequired"));

        RuleFor(x => x.FollowingId)
            .NotEmpty()
            .WithMessage(messageProvider.GetMessage("ValidationMessages", "Social:FollowingIdRequired"));

        RuleFor(x => x).Must(x => x.FollowerId != x.FollowingId)
            .WithMessage(messageProvider.GetMessage("ValidationMessages", "Social:CannotUnfollowYourself"));
    }
}

public class UnfollowUserCommandHandler : IRequestHandler<UnfollowUserCommand, Result<UnfollowUserResponse>>
{
    private readonly ISocialDbContext _context;
    private readonly IEventBus _eventBus;
    private readonly ILocalizedMessageProvider _messageProvider;

    public UnfollowUserCommandHandler(
        ISocialDbContext context,
        IEventBus eventBus,
        ILocalizedMessageProvider messageProvider)
    {
        _context = context;
        _eventBus = eventBus;
        _messageProvider = messageProvider;
    }

    public async Task<Result<UnfollowUserResponse>> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
    {
        var follow = await _context.Follows.AsQueryable()
            .FirstOrDefaultAsync(f => f.FollowerId == request.FollowerId && f.FollowingId == request.FollowingId, cancellationToken);

        if (follow == null)
        {
            var notFollowingMsg = _messageProvider.GetUserMessage("Social:NotFollowingThisUser");
            return Result<UnfollowUserResponse>.Failure(notFollowingMsg, HttpStatusCode.BadRequest);
        }

        _context.Follows.Remove(follow);
        await _context.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new UserUnfollowedIntegrationEvent(
            request.FollowerId,
            request.FollowingId,
            DateTime.UtcNow
        ), cancellationToken);

        var response = new UnfollowUserResponse(true);
        return Result<UnfollowUserResponse>.Success(response);
    }
}