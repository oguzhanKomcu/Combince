using Combince.Modules.Social.Core.Abstractions;
using Combince.Modules.Social.Core.Entities;

using Combince.Shared.Core.Events.Modules.User;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Combince.Modules.Social.Infrastructure.Consumers;

public class UserRegisteredIntegrationConsumer : IConsumer<UserRegisteredIntegrationEvent>
{
    private readonly ISocialDbContext _context;

    public UserRegisteredIntegrationConsumer(ISocialDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<UserRegisteredIntegrationEvent> context)
    {
        var message = context.Message;
        var exists = await _context.SocialUsers
            .AnyAsync(u => u.UserId == message.UserId);

        if (exists) return;

     
        var socialUser = new SocialUser(
            message.UserId,
            message.Username,
            message.ProfilePictureUrl);

        _context.SocialUsers.Add(socialUser);
        await _context.SaveChangesAsync(context.CancellationToken);
    }
}