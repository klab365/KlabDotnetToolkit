using System.Threading;
using System.Threading.Tasks;

namespace Klab.Toolkit.Event;

/// <summary>
/// Basic implementation of <see cref="IEventBus"/>
///
/// It uses an <see cref="IEventQueue"/> to enqueue events
/// </summary>
internal sealed class EventBus : IEventBus
{
    private readonly IEventQueue _messageQueue;

    public EventBus(IEventQueue messageQueue)
    {
        _messageQueue = messageQueue;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        await _messageQueue.EnqueueAsync(@event, cancellationToken);
    }
}
