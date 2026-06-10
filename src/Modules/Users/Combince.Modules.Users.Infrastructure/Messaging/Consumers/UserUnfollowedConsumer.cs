using Combince.Modules.Users.Core.Abstractions;
using Combince.Shared.Core.Events.Modules;
using Combince.Shared.Core.Events.Modules.Follow;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Combince.Modules.Users.Infrastructure.Consumers;

public class UserUnfollowedConsumer : IConsumer<UserUnfollowedIntegrationEvent>
{
    private readonly IUsersDbContext _context;

    public UserUnfollowedConsumer(IUsersDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<UserUnfollowedIntegrationEvent> context)
    {
        var message = context.Message;

        var follower = await _context.Users.AsQueryable()
            .FirstOrDefaultAsync(u => u.Id == message.FollowerId, context.CancellationToken);

        if (follower != null && follower.FollowingCount > 0)
        {
            follower.FollowingCount--;
        }

        var following = await _context.Users.AsQueryable()
            .FirstOrDefaultAsync(u => u.Id == message.FollowingId, context.CancellationToken);

        if (following != null && following.FollowersCount > 0)
        {
            following.FollowersCount--;
        }

        await _context.SaveChangesAsync(context.CancellationToken);
    }
}