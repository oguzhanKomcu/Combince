using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Combince.Modules.Ratings.Core.Abstractions;
using Combince.Modules.Ratings.Core.Common;
using Combince.Modules.Ratings.Core.Entities;
using Combince.Shared.Core.Abstractions;
using Combince.Shared.Core.Events.Modules.PostRating;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.Ratings.Core.Features.Ratings.Commands.RatePost;

public record RatePostCommand(Guid PostId, Guid UserId, int Score) : IRequest<Result<RatingSuccessResponse>>;

public record RatingSuccessResponse(string Message, bool Success);

public class RatePostCommandHandler : IRequestHandler<RatePostCommand, Result<RatingSuccessResponse>>
{
    private readonly IRatingsDbContext _context;
    private readonly IEventBus _eventBus;
    private readonly ILocalizedMessageProvider _messageProvider;

    public RatePostCommandHandler(
        IRatingsDbContext context,
        IEventBus eventBus,
        ILocalizedMessageProvider messageProvider)
    {
        _context = context;
        _eventBus = eventBus;
        _messageProvider = messageProvider;
    }

    public async Task<Result<RatingSuccessResponse>> Handle(RatePostCommand request, CancellationToken cancellationToken)
    {
        var existingRating = await _context.PostRatings
            .FirstOrDefaultAsync(r => r.PostId == request.PostId && r.UserId == request.UserId, cancellationToken);

        if (existingRating != null)
        {
            var alreadyRatedMessage = _messageProvider.GetMessage("UserMessages", "Ratings:AlreadyRated");
            return Result<RatingSuccessResponse>.Failure(alreadyRatedMessage, System.Net.HttpStatusCode.BadRequest);
        }

        var newRating = new PostRating(request.PostId, request.UserId, request.Score);
        _context.PostRatings.Add(newRating);

        await _context.SaveChangesAsync(cancellationToken);

        var allRatings = await _context.PostRatings
            .Where(r => r.PostId == request.PostId)
            .Select(r => r.Score)
            .ToListAsync(cancellationToken);

        int totalVotes = allRatings.Count;
        double averageRating = totalVotes > 0 ? allRatings.Average() : 0;

        await _eventBus.PublishAsync(new PostRatingUpdatedIntegrationEvent(
            request.PostId,
            averageRating,
            totalVotes
        ), cancellationToken);

        var successMessage = _messageProvider.GetMessage("UserMessages", "Ratings:RatingSavedSuccessfully");

        return Result<RatingSuccessResponse>.Success(new RatingSuccessResponse(successMessage, true));
    }
}