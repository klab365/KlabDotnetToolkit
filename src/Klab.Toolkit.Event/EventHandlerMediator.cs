using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;
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

    public async Task<Result> PublishToHandlersAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
    {
        try
        {
            EventHandlerWrapper eventHandler = GetEventHandler(@event);
            IEnumerable<EventHandlerExecutor> handlers = eventHandler.GetHandlers(_serviceProvider);
            await TaskWhenAllPublisher.Publish(handlers, @event, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            InformativeError err = EventErrors.EventHandlerNotFound(@event.GetType());
            err.StackTrace = ex.StackTrace;
            return Result.Failure(err);
        }
    }

    public async Task<Result<TResponse>> SendToHanderAsync<TRequest, TResponse>(IRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
        where TResponse : notnull
    {
        IRequestHandler<TRequest, TResponse> handler = _serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        return await handler.HandleAsync((TRequest)request, cancellationToken);
    }

    public async Task<Result> SendToHanderAsync<TRequest>(TRequest request, CancellationToken cancellationToken) where TRequest : IRequest
    {
        IRequestHandler<TRequest> handler = _serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();
        return await handler.HandleAsync(request, cancellationToken);
    }

    private EventHandlerWrapper GetEventHandler(IEvent @event)
    {
        return _eventHandlers.GetOrAdd(@event.GetType(), eventType =>
        {
            Type wrapperType = typeof(EventHandlerWrapper<>).MakeGenericType(eventType);
            object? wrapper = _serviceProvider.GetService(wrapperType);
            if (wrapper == null)
            {
                throw new KeyNotFoundException($"No event handler found for event type {eventType}");
            }

            return (EventHandlerWrapper)wrapper;
        });
    }
}
