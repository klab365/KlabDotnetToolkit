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
    public Task<IResult[]> Handle(IEnumerable<EventHandlerExecutor> handlerExecutors, IEvent @event, CancellationToken cancellationToken)
    {
        Task<IResult>[] tasks = handlerExecutors
            .Select(handler => handler.HandlerCallback(@event, cancellationToken))
            .ToArray();

        return Task.WhenAll(tasks);
    }
}
