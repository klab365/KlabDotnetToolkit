using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.Event;

/// <summary>
/// Wrapper for event handlers
/// </summary>
internal abstract class EventHandlerWrapper
{
    public abstract IEnumerable<EventHandlerExecutor> GetHandlers(IServiceProvider serviceProvider);
}

/// <summary>
/// Generic event handler wrapper
/// </summary>
/// <typeparam name="TEvent"></typeparam>
internal class EventHandlerWrapper<TEvent> : EventHandlerWrapper where TEvent : IEvent
{
    public override IEnumerable<EventHandlerExecutor> GetHandlers(IServiceProvider serviceProvider)
    {
        IEnumerable<EventHandlerExecutor> handlers = serviceProvider
                .GetServices<IEventHandler<TEvent>>()
                .Select(static handler => new EventHandlerExecutor(handler, (e, ct) => handler.Handle((TEvent)e, ct)));

        return handlers;
    }
}

/// <summary>
/// Event Handler Executor record
/// </summary>
internal record EventHandlerExecutor
{
    public object HandlerInstance { get; set; }
    public Func<IEvent, CancellationToken, Task<Result>> HandlerCallback { get; set; }

    public EventHandlerExecutor(object handlerInstance, Func<IEvent, CancellationToken, Task<Result>> handlerCallback)
    {
        HandlerInstance = handlerInstance;
        HandlerCallback = handlerCallback;
    }
}
