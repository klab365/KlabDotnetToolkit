using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.Event;

/// <summary>
/// Event Handler Mediator
/// This implementation is inspired by the MediatR library.
/// </summary>
internal class EventHandlerMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, EventHandlerWrapper> _eventHandlers = new();

    public EventHandlerMediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
    {
        EventHandlerWrapper eventHandler = GetEventHandler(@event);
        await eventHandler.Handle(@event, _serviceProvider, TaskWhenAllPublisher.Publish, cancellationToken);
    }

    private EventHandlerWrapper GetEventHandler(IEvent @event)
    {
        return _eventHandlers.GetOrAdd(@event.GetType(), eventType =>
        {

            Type wrapperType = typeof(EventHandlerWrapper<>).MakeGenericType(eventType);
            object wrapper = _serviceProvider.GetRequiredService(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper for type {eventType}");
            return (EventHandlerWrapper)wrapper;
        });
    }
}
