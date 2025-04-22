using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;

namespace Klab.Toolkit.Event;


/// <summary>
/// This is a publisher strategy that uses Task.WhenAll to publish events
/// </summary>
internal class TaskWhenAllPublisher : IEventHandlerProcessingStrategy
{
    public Task<Result[]> Handle(IEnumerable<EventHandlerExecutor> handlerExecutors, EventBase @event, CancellationToken cancellationToken)
    {
        Task<Result>[] tasks = handlerExecutors
            .Select(handler => handler.HandlerCallback(@event, cancellationToken))
            .ToArray();

        return Task.WhenAll(tasks);
    }
}
