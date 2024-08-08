using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.Event;

/// <summary>
/// Wrapper for event handlers
/// </summary>
internal abstract class EventHandlerWrapper
{
    public abstract Task Handle(
        IEvent @event,
        IServiceProvider serviceFactory,
        Func<IEnumerable<EventHandlerExecutor>, IEvent, CancellationToken, Task> publish,
        CancellationToken cancellationToken);
}

/// <summary>
/// Generic event handler wrapper
/// </summary>
/// <typeparam name="TEvent"></typeparam>
internal class EventHandlerWrapper<TEvent> : EventHandlerWrapper
    where TEvent : IEvent
{
    public override Task Handle(
        IEvent @event,
        IServiceProvider serviceFactory,
        Func<IEnumerable<EventHandlerExecutor>, IEvent, CancellationToken, Task> publish,
        CancellationToken cancellationToken)
    {
        IEnumerable<EventHandlerExecutor> handlers = serviceFactory
            .GetServices<IEventHandler<TEvent>>()
            .Select(static handler => new EventHandlerExecutor(handler, (e, ct) => handler.Handle((TEvent)e, ct)));

        return publish(handlers, @event, cancellationToken);
    }
}

/// <summary>
/// Event Handler Executor record
/// </summary>
internal record EventHandlerExecutor
{
    public object HandlerInstance { get; set; }
    public Func<IEvent, CancellationToken, Task> HandlerCallback { get; set; }

    public EventHandlerExecutor(object handlerInstance, Func<IEvent, CancellationToken, Task> handlerCallback)
    {
        HandlerInstance = handlerInstance;
        HandlerCallback = handlerCallback;
    }
}
