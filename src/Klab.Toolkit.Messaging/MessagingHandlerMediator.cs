using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Klab.Toolkit.Messaging;

/// <summary>
/// Messaging Handler Mediator
/// This implementation is inspired by the MediatR library.
/// </summary>
internal sealed class MessagingHandlerMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MessagingHandlerMediator> _logger;
    private readonly IMessagingLogger _messagingLogger;
    private readonly Dictionary<Type, IEventHandlerWrapperRegistration> _eventHandlers;
    private readonly Dictionary<(Type RequestType, Type ResponseType), IRequestResponseHandlerWrapperRegistration> _requestHandlers;
    private readonly Dictionary<(Type RequestType, Type ResponseType), IStreamRequestResponseHandlerWrapperRegistration> _streamRequestHandlers;
    private readonly IEventHandlerProcessingStrategy _eventProcessingStrategy;

    public MessagingHandlerMediator(IServiceProvider serviceProvider, ILogger<MessagingHandlerMediator> logger, IMessagingLogger messagingLogger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _messagingLogger = messagingLogger;
        _eventProcessingStrategy = serviceProvider.GetRequiredService<IEventHandlerProcessingStrategy>();
        _eventHandlers = serviceProvider.GetServices<IEventHandlerWrapperRegistration>().GroupBy(static registration => registration.EventType).ToDictionary(static group => group.Key, static group => group.First());
        _requestHandlers = serviceProvider.GetServices<IRequestResponseHandlerWrapperRegistration>().GroupBy(static registration => (registration.RequestType, registration.ResponseType)).ToDictionary(static group => group.Key, static group => group.First());
        _streamRequestHandlers = serviceProvider.GetServices<IStreamRequestResponseHandlerWrapperRegistration>().GroupBy(static registration => (registration.RequestType, registration.ResponseType)).ToDictionary(static group => group.Key, static group => group.First());
    }

    public async Task<Result[]> PublishToHandlersAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : EventBase
    {
        try
        {
            if (!_eventHandlers.TryGetValue(@event.GetType(), out IEventHandlerWrapperRegistration? eventHandler))
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("No event handler found for event type {EventType}", @event.GetType());
                }
                return [];
            }

            return await _eventProcessingStrategy.Handle(eventHandler.GetHandlers(_serviceProvider, @event), @event, cancellationToken);
        }
        catch (Exception ex)
        {
            IError err = Error.FromException(ex, MessagingErrors.Keys.GenericEventErrorKey);
            return [Result.Failure(err)];
        }
    }

    public async Task<TResponse> SendToHanderAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        where TResponse : notnull
    {
        if (!_requestHandlers.TryGetValue((request.GetType(), typeof(TResponse)), out IRequestResponseHandlerWrapperRegistration? requestHandler))
        {
            throw new KeyNotFoundException($"No request handler found for request type {request.GetType()}");
        }

        TResponse response = (TResponse)await requestHandler.HandleAsync(request, _serviceProvider, cancellationToken);
        await _messagingLogger.LogCommandAsync(request.GetType(), request, response);
        return response;
    }

    public async IAsyncEnumerable<TResponse> SendToStreamHandlerAsync<TResponse>(IStreamRequest<TResponse> request, [EnumeratorCancellation] CancellationToken cancellationToken)
        where TResponse : notnull
    {
        if (!_streamRequestHandlers.TryGetValue((request.GetType(), typeof(TResponse)), out IStreamRequestResponseHandlerWrapperRegistration? requestHandler))
        {
            throw new KeyNotFoundException($"No stream request handler found for request type {request.GetType()}");
        }

        await foreach (TResponse item in CastAsync<TResponse>(requestHandler.HandleAsync(request, _serviceProvider, cancellationToken)))
        {
            await _messagingLogger.LogStreamRequestAsync(request.GetType(), request, item);
            yield return item;
        }
    }

    private static async IAsyncEnumerable<TResponse> CastAsync<TResponse>(IAsyncEnumerable<object> items)
    {
        await foreach (object item in items)
        {
            yield return (TResponse)item;
        }
    }
}
