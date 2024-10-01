using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Klab.Toolkit.Event;

/// <summary>
/// Interface for a event queue
/// </summary>
public interface IEventQueue
{
    /// <summary>
    /// Enqueues an event
    /// </summary>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task EnqueueAsync(IEvent @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dequeues events
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IAsyncEnumerable<IEvent> DequeueAsync(CancellationToken cancellationToken = default);
}

