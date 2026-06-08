using System.Threading;
using System.Threading.Tasks;
using Combince.Shared.Core.Abstractions;
using MassTransit;

namespace Combince.Shared.Infrastructure.Messaging;

public class EventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public EventBus(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
    {
        await _publishEndpoint.Publish(@event, cancellationToken);
    }
}