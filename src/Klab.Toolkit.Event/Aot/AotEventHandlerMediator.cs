using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Klab.Toolkit.Event.Aot;

#if NET8_0_OR_GREATER
/// <summary>
/// AOT-compatible event handler mediator.
/// Uses static delegate factories instead of runtime MakeGenericType.
/// 
/// Usage (net8.0+ only):
/// 1. Call AotEventHandlerMediator.RegisterEventHandler&lt;TEvent&gt;() in your composition root
/// 2. Replace EventHandlerMediator with AotEventHandlerMediator in DI
/// </summary>
public class AotEventHandlerMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventHandlerMediator> _logger;
    private readonly IEventBusLogger _eventBusLogger;
    private readonly IEventHandlerProcessingStrategy _eventProcessingStrategy;

    internal static readonly Dictionary<Type, Func<IServiceProvider, IEnumerable<EventHandlerExecutor>>> EventHandlerFactories = new();
    internal static readonly Dictionary<Type, Func<IServiceProvider, RequestResponseHandlerWrapper>> RequestHandlerFactories = new();
    internal static readonly Dictionary<Type, Func<IServiceProvider, StreamRequestResponseHandlerWrapper>> StreamRequestHandlerFactories = new();

    public AotEventHandlerMediator(
        IServiceProvider serviceProvider,
        ILogger<EventHandlerMediator> logger,
        IEventBusLogger eventBusLogger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _eventBusLogger = eventBusLogger;
        _eventProcessingStrategy = serviceProvider.GetRequiredService<IEventHandlerProcessingStrategy>();
    }

    public static void RegisterEventHandler<TEvent>()
        where TEvent : EventBase
    {
        EventHandlerFactories[typeof(TEvent)] = sp =>
        {
            IEnumerable<IEventHandler<TEvent>> handlers = sp.GetServices<IEventHandler<TEvent>>();
            var executors = new List<EventHandlerExecutor>();
            foreach (IEventHandler<TEvent> handler in handlers)
            {
                executors.Add(new EventHandlerExecutor(handler, (e, ct) => handler.Handle((TEvent)e, ct)));
            }
            return executors;
        };
    }

    public static void RegisterRequestHandler<TRequest, TResponse>()
        where TRequest : IRequest<TResponse>
        where TResponse : notnull
    {
        RequestHandlerFactories[typeof(TRequest)] = _ => new RequestResponseHandlerWrapper<TRequest, TResponse>();
    }

    public static void RegisterStreamRequestHandler<TRequest, TResponse>()
        where TRequest : IStreamRequest<TResponse>
        where TResponse : notnull
    {
        StreamRequestHandlerFactories[typeof(TRequest)] = _ => new StreamRequestResponseHandlerWrapper<TRequest, TResponse>();
    }

    public async Task<Result[]> PublishToHandlersAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : EventBase
    {
        try
        {
            if (!EventHandlerFactories.TryGetValue(typeof(TEvent), out Func<IServiceProvider, IEnumerable<EventHandlerExecutor>>? factory))
            {
                _logger.LogDebug("No event handler found for event type {EventType}", typeof(TEvent).Name);
                return [];
            }

            IEnumerable<EventHandlerExecutor> handlers = factory(_serviceProvider);
            return await _eventProcessingStrategy.Handle(handlers, @event, cancellationToken);
        }
        catch (Exception ex)
        {
            IError err = Error.FromException(ex, EventErrors.Keys.GenericEventErrorKey);
            return [Result.Failure(err)];
        }
    }

    public async Task<TResponse> SendToHandlerAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        where TResponse : notnull
    {
        if (!RequestHandlerFactories.TryGetValue(request.GetType(), out Func<IServiceProvider, RequestResponseHandlerWrapper>? factory))
        {
            throw new KeyNotFoundException($"No request handler found for request type {request.GetType()}");
        }

        RequestResponseHandlerWrapper wrapper = factory(_serviceProvider);
        TResponse response = (TResponse)await wrapper.HandleAsync(request, _serviceProvider, cancellationToken);
        await _eventBusLogger.LogCommandAsync(request.GetType(), request, response);
        return response;
    }

    public async IAsyncEnumerable<TResponse> SendToStreamHandlerAsync<TResponse>(
        IStreamRequest<TResponse> request,
        CancellationToken cancellationToken)
        where TResponse : notnull
    {
        if (!StreamRequestHandlerFactories.TryGetValue(request.GetType(), out Func<IServiceProvider, StreamRequestResponseHandlerWrapper>? factory))
        {
            throw new KeyNotFoundException($"No stream request handler found for request type {request.GetType()}");
        }

        StreamRequestResponseHandlerWrapper wrapper = factory(_serviceProvider);

        await foreach (TResponse item in wrapper.HandleAsync(request, _serviceProvider, cancellationToken))
        {
            await _eventBusLogger.LogStreamRequestAsync(request.GetType(), request, item);
            yield return item;
        }
    }
}
#endif