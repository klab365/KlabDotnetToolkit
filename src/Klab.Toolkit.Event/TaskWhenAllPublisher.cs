using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Klab.Toolkit.Event;


/// <summary>
/// This is a publisher strategy that uses Task.WhenAll to publish events
/// </summary>
internal static class TaskWhenAllPublisher
{
    public static Task Publish(IEnumerable<EventHandlerExecutor> handlerExecutors, IEvent @event, CancellationToken cancellationToken)
    {
        Task[] tasks = handlerExecutors
            .Select(handler => handler.HandlerCallback(@event, cancellationToken))
            .ToArray();

        return Task.WhenAll(tasks);
    }
}
