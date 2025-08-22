using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Klab.Toolkit.Event;

/// <summary>
/// Event Handler Mediator
/// This implementation is inspired by the MediatR library.
/// </summary>
internal class EventHandlerMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventHandlerMediator> _logger;
    private readonly ConcurrentDictionary<Type, EventHandlerWrapper> _eventHandlers = new();
    private readonly ConcurrentDictionary<Type, RequestResponseHandlerWrapper> _requestHandlers = new();
    private readonly ConcurrentDictionary<Type, StreamRequestResponseHandlerWrapper> _streamRequestHandlers = new();
    private readonly IEventHandlerProcessingStrategy _eventProcessingStrategy;

    public EventHandlerMediator(IServiceProvider serviceProvider, ILogger<EventHandlerMediator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _eventProcessingStrategy = serviceProvider.GetRequiredService<IEventHandlerProcessingStrategy>();
    }

    public async Task<Result[]> PublishToHandlersAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : EventBase
    {
        try
        {
            EventHandlerWrapper? eventHandler = GetEventHandler(@event);
            if (eventHandler is null)
            {
                _logger.LogDebug("No event handler found for event type {EventType}", @event.GetType());
                return [];
            }

            IEnumerable<EventHandlerExecutor> handlers = eventHandler.GetHandlers(_serviceProvider);
            return await _eventProcessingStrategy.Handle(handlers, @event, cancellationToken);
        }
        catch (Exception ex)
        {
            Error err = Error.FromException(ex, EventErrors.Keys.GenericEventErrorKey);
            return [Result.Failure(err)];
        }
    }

    public async Task<TResponse> SendToHanderAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        where TResponse : notnull
    {
        RequestResponseHandlerWrapper requestHandler = GetRequestHandlerWrapper(request);
        return (TResponse)await requestHandler.HandleAsync(request, _serviceProvider, cancellationToken);
    }

    public async IAsyncEnumerable<TResponse> SendToStreamHandlerAsync<TResponse>(IStreamRequest<TResponse> request, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        StreamRequestResponseHandlerWrapper requestHandler = GetStreamRequestHandlerWrapper(request);

        await foreach (TResponse item in requestHandler.HandleAsync(request, _serviceProvider, cancellationToken))
        {
            yield return item;
        }
    }

    private EventHandlerWrapper? GetEventHandler(EventBase @event)
    {
        EventHandlerWrapper? res = _eventHandlers.GetOrAdd(@event.GetType(), eventType =>
        {
            Type wrapperType = typeof(EventHandlerWrapper<>).MakeGenericType(eventType);
            object? wrapper = _serviceProvider.GetService(wrapperType);
            return (EventHandlerWrapper)wrapper;
        });

        return res;
    }

    private RequestResponseHandlerWrapper GetRequestHandlerWrapper<TResponse>(IRequest<TResponse> request)
    {
        return _requestHandlers.GetOrAdd(request.GetType(), requestType =>
        {
            Type wrapperType = typeof(RequestResponseHandlerWrapper<,>).MakeGenericType(requestType, typeof(TResponse));
            object? wrapper = _serviceProvider.GetService(wrapperType);
            if (wrapper == null)
            {
                throw new KeyNotFoundException($"No request handler found for request type {requestType}");
            }

            return (RequestResponseHandlerWrapper)wrapper;
        });
    }

    private StreamRequestResponseHandlerWrapper GetStreamRequestHandlerWrapper<TResponse>(IStreamRequest<TResponse> request)
    {
        return _streamRequestHandlers.GetOrAdd(request.GetType(), requestType =>
        {
            Type wrapperType = typeof(StreamRequestResponseHandlerWrapper<,>).MakeGenericType(requestType, typeof(TResponse));
            object? wrapper = _serviceProvider.GetService(wrapperType);
            if (wrapper == null)
            {
                throw new KeyNotFoundException($"No stream request handler found for request type {requestType}");
            }

            return (StreamRequestResponseHandlerWrapper)wrapper;
        });
    }
}
