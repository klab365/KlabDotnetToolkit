using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;

namespace Klab.Toolkit.Event;

/// <summary>
/// Basic implementation of <see cref="IEventBus"/>
/// </summary>
internal sealed class EventBus : IEventBus
{
    private readonly EventHandlerMediator _eventHandlerMediator;
    private readonly ConcurrentDictionary<Type, ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>>> _localEventHandlers = new();

    public IEventQueue MessageQueue { get; }

    public EventBus(IEventQueue messageQueue, EventHandlerMediator eventHandlerMediator)
    {
        MessageQueue = messageQueue;
        _eventHandlerMediator = eventHandlerMediator;
    }

    public ConcurrentDictionary<Type, ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>>> GetLocalEventHandlers()
    {
        return _localEventHandlers;
    }

    public async Task<Result> PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : EventBase
    {
        try
        {
            await MessageQueue.EnqueueAsync(@event, cancellationToken);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            return Result.Success(); // If the operation was canceled, we consider it a success in this context
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Create("EventBus.PublishAsync", ex.Message));
        }
    }

    public Result Subscribe<TEvent>(Func<TEvent, CancellationToken, Task<Result>> handler) where TEvent : EventBase
    {
        ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>> handlers = _localEventHandlers.GetOrAdd(typeof(TEvent), _ => []);

        int handlerHash = CalculateHashOfHandler(handler);
        if (handlers.Any(x => x.Key == handlerHash))
        {
            return Result.Failure(Error.Create(string.Empty, "Handler already exists"));
        }

        handlers.Add(new(handlerHash, (@event, token) => handler((TEvent)@event, token)));
        return Result.Success();
    }

    public Result Unsubscribe<TEvent>(Func<TEvent, CancellationToken, Task<Result>> handler) where TEvent : EventBase
    {
        if (!_localEventHandlers.TryGetValue(typeof(TEvent), out ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>>? handlers))
        {
            return Result.Failure(Error.Create(string.Empty, "No handler found for the event type"));
        }

        int handlerHash = CalculateHashOfHandler(handler);

        // Spin-wait retry loop to handle TryUpdate race conditions
        SpinWait spinWait = new();
        while (true)
        {
            if (!_localEventHandlers.TryGetValue(typeof(TEvent), out handlers))
            {
                // Handler list was removed, consider it success
                return Result.Success();
            }

            ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>> newBag = [.. handlers.Where(x => x.Key != handlerHash)];

            if (_localEventHandlers.TryUpdate(typeof(TEvent), newBag, handlers))
            {
                return Result.Success();
            }

            // Spin-wait before retrying to reduce contention
            spinWait.SpinOnce();
        }
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        where TResponse : notnull
    {
        return _eventHandlerMediator.SendToHanderAsync(request, cancellationToken);
    }

    public IAsyncEnumerable<TResponse> Stream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) where TResponse : notnull
    {
        return _eventHandlerMediator.SendToStreamHandlerAsync(request, cancellationToken);
    }

    private static int CalculateHashOfHandler<TEvent>(Func<TEvent, CancellationToken, Task<Result>> handler) where TEvent : EventBase
    {
        return handler.Method.GetHashCode() ^ handler.Target?.GetHashCode() ?? throw new InvalidOperationException("Handler is not valid");
    }
}
