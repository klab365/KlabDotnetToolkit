using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Klab.Toolkit.Event;

/// <summary>
/// In-memory store for events
/// It uses <see cref="Channel"/> to store events. These are very fast and efficient.
/// </summary>
internal sealed class InMemoryMessageQueue : IEventQueue
{
    private readonly Channel<IEvent> _channel = Channel.CreateUnbounded<IEvent>();

    /// <inheritdoc/>
    public IAsyncEnumerable<IEvent> DequeueAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task EnqueueAsync(IEvent @event, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(@event, cancellationToken);
    }
}
