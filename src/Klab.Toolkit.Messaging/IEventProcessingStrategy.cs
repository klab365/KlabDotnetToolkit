using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;

namespace Klab.Toolkit.Messaging;

internal interface IEventHandlerProcessingStrategy
{
    Task<Result[]> Handle(IEnumerable<EventHandlerExecutor> handlerExecutors, EventBase @event, CancellationToken cancellationToken);
}
