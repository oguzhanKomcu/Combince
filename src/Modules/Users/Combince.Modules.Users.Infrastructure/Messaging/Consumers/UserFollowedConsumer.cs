using Combince.Modules.Social.Core.Features.Follows.Commands.FollowUser;
using Combince.Modules.Users.Core.Abstractions;
using Combince.Shared.Core.Events.Modules;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Combince.Modules.Users.Infrastructure.Consumers;

public class UserFollowedConsumer : IConsumer<UserFollowedIntegrationEvent>
{
    private readonly IUsersDbContext _context;

    public UserFollowedConsumer(IUsersDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<UserFollowedIntegrationEvent> context)
    {
        var message = context.Message;

        var follower = await _context.Users.AsQueryable()
            .FirstOrDefaultAsync(u => u.Id == message.FollowerId, context.CancellationToken);

        if (follower != null)
        {
            follower.FollowingCount++;
        }

        var following = await _context.Users.AsQueryable()
            .FirstOrDefaultAsync(u => u.Id == message.FollowingId, context.CancellationToken);

        if (following != null)
        {
            following.FollowersCount++;
        }

        await _context.SaveChangesAsync(context.CancellationToken);
    }
}