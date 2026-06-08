using System.Threading;
using System.Threading.Tasks;

namespace Combince.Shared.Core.Abstractions;

public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;
}