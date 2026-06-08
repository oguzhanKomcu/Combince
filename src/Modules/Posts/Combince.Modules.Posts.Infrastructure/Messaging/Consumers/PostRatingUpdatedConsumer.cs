using Combince.Modules.Posts.Core.Abstractions;
using Combince.Shared.Core.Events;
using Combince.Shared.Core.Events.Modules.PostRating;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Combince.Modules.Posts.Infrastructure.Messaging.Consumers;

public class PostRatingUpdatedConsumer : IConsumer<PostRatingUpdatedIntegrationEvent>
{
    private readonly IPostsDbContext _context;

    public PostRatingUpdatedConsumer(IPostsDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<PostRatingUpdatedIntegrationEvent> context)
    {
        var message = context.Message;

        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == message.PostId);

        if (post != null)
        {
            post.UpdateRatingSummary(message.AverageRating, message.TotalVotes);
            await _context.SaveChangesAsync(context.CancellationToken);
        }
    }
}