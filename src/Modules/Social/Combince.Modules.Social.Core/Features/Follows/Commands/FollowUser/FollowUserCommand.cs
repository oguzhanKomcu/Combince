using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Combince.Modules.Social.Core.Entities;
using Combince.Modules.Social.Core.Common;
using Combince.Modules.Social.Core.Abstractions;
using Combince.Shared.Core.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.Social.Core.Features.Follows.Commands.FollowUser;

public record UserFollowedIntegrationEvent(Guid FollowId, Guid FollowerId, Guid FollowingId, DateTime FollowedAt);

public record FollowUserCommand(Guid FollowerId, Guid FollowingId) : IRequest<Result<FollowUserResponse>>;

public record FollowUserResponse(Guid FollowId);

public class FollowUserCommandValidator : AbstractValidator<FollowUserCommand>
{
    public FollowUserCommandValidator(ILocalizedMessageProvider messageProvider)
    {
        RuleFor(x => x.FollowerId)
            .NotEmpty()
            .WithMessage(messageProvider.GetMessage("ValidationMessages", "Social:FollowerIdRequired"));

        RuleFor(x => x.FollowingId)
            .NotEmpty()
            .WithMessage(messageProvider.GetMessage("ValidationMessages", "Social:FollowingIdRequired"));

        RuleFor(x => x).Must(x => x.FollowerId != x.FollowingId)
            .WithMessage(messageProvider.GetMessage("ValidationMessages", "Social:CannotFollowYourself"));
    }
}

public class FollowUserCommandHandler : IRequestHandler<FollowUserCommand, Result<FollowUserResponse>>
{
    private readonly ISocialDbContext _context;
    private readonly IEventBus _eventBus;
    private readonly ILocalizedMessageProvider _messageProvider;

    public FollowUserCommandHandler(
        ISocialDbContext context,
        IEventBus eventBus,
        ILocalizedMessageProvider messageProvider)
    {
        _context = context;
        _eventBus = eventBus;
        _messageProvider = messageProvider;
    }

    public async Task<Result<FollowUserResponse>> Handle(FollowUserCommand request, CancellationToken cancellationToken)
    {
        bool isAlreadyFollowing = await _context.Follows
            .AnyAsync(f => f.FollowerId == request.FollowerId && f.FollowingId == request.FollowingId, cancellationToken);

        if (isAlreadyFollowing)
        {
            var alreadyFollowingMsg = _messageProvider.GetUserMessage("Social:AlreadyFollowingThisUser");
            return Result<FollowUserResponse>.Failure(alreadyFollowingMsg, HttpStatusCode.BadRequest);
        }

        var follow = new Follow(request.FollowerId, request.FollowingId);

        await _context.Follows.AddAsync(follow, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new UserFollowedIntegrationEvent(
            follow.Id,
            follow.FollowerId,
            follow.FollowingId,
            follow.CreatedAt
        ), cancellationToken);

        var response = new FollowUserResponse(follow.Id);
        return Result<FollowUserResponse>.Success(response);
    }
}