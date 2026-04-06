using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    private readonly IEventBusLogger _eventBusLogger;
    private readonly ConcurrentDictionary<Type, EventHandlerWrapper> _eventHandlers = new();
    private readonly ConcurrentDictionary<Type, RequestResponseHandlerWrapper> _requestHandlers = new();
    private readonly ConcurrentDictionary<Type, StreamRequestResponseHandlerWrapper> _streamRequestHandlers = new();
    private readonly IEventHandlerProcessingStrategy _eventProcessingStrategy;

    public EventHandlerMediator(IServiceProvider serviceProvider, ILogger<EventHandlerMediator> logger, IEventBusLogger eventBusLogger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _eventBusLogger = eventBusLogger;
        _eventProcessingStrategy = serviceProvider.GetRequiredService<IEventHandlerProcessingStrategy>();
    }

    public async Task<Result[]> PublishToHandlersAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : EventBase
    {
        try
        {
            EventHandlerWrapper? eventHandler = GetEventHandler(@event);
            if (eventHandler is null)
            {
#pragma warning disable CA1873 // Avoid potentially expensive logging
                _logger.LogDebug("No event handler found for event type {EventType}", @event.GetType());
#pragma warning restore CA1873 // Avoid potentially expensive logging
                return [];
            }

            IEnumerable<EventHandlerExecutor> handlers = eventHandler.GetHandlers(_serviceProvider);
            return await _eventProcessingStrategy.Handle(handlers, @event, cancellationToken);
        }
        catch (Exception ex)
        {
            IError err = Error.FromException(ex, EventErrors.Keys.GenericEventErrorKey);
            return [Result.Failure(err)];
        }
    }

    public async Task<TResponse> SendToHanderAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        where TResponse : notnull
    {
        RequestResponseHandlerWrapper requestHandler = GetRequestHandlerWrapper(request);
        TResponse response = (TResponse)await requestHandler.HandleAsync(request, _serviceProvider, cancellationToken);
        await _eventBusLogger.LogCommandAsync(request.GetType(), request, response);
        return response;
    }

    public async IAsyncEnumerable<TResponse> SendToStreamHandlerAsync<TResponse>(IStreamRequest<TResponse> request, [EnumeratorCancellation] CancellationToken cancellationToken)
        where TResponse : notnull
    {
        StreamRequestResponseHandlerWrapper requestHandler = GetStreamRequestHandlerWrapper(request);

        await foreach (TResponse item in requestHandler.HandleAsync(request, _serviceProvider, cancellationToken))
        {
            await _eventBusLogger.LogStreamRequestAsync(request.GetType(), request, item);
            yield return item;
        }
    }

    /// <summary>
    /// Gets the event handler for the specified event type.
    /// </summary>
    /// <remarks>
    /// This method uses reflection to create generic wrapper types at runtime.
    /// It is marked with [RequiresUnreferencedCode] because MakeGenericType cannot be
    /// statically analyzed by the AOT compiler. For full AOT compatibility, consider
    /// using source generators to pre-generate these lookups.
    /// </remarks>
    [RequiresUnreferencedCode("Uses MakeGenericType for runtime type creation which is not AOT-safe.")]
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

    /// <summary>
    /// Gets the request handler wrapper for the specified request type.
    /// </summary>
    /// <remarks>
    /// This method uses reflection to create generic wrapper types at runtime.
    /// It is marked with [RequiresUnreferencedCode] because MakeGenericType cannot be
    /// statically analyzed by the AOT compiler. For full AOT compatibility, consider
    /// using source generators to pre-generate these lookups.
    /// </remarks>
    [RequiresUnreferencedCode("Uses MakeGenericType for runtime type creation which is not AOT-safe.")]
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

    /// <summary>
    /// Gets the stream request handler wrapper for the specified request type.
    /// </summary>
    /// <remarks>
    /// This method uses reflection to create generic wrapper types at runtime.
    /// It is marked with [RequiresUnreferencedCode] because MakeGenericType cannot be
    /// statically analyzed by the AOT compiler. For full AOT compatibility, consider
    /// using source generators to pre-generate these lookups.
    /// </remarks>
    [RequiresUnreferencedCode("Uses MakeGenericType for runtime type creation which is not AOT-safe.")]
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
