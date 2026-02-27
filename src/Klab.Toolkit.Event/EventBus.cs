using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;

namespace Klab.Toolkit.Event;

internal sealed class EventBus : IEventBus
{
    private readonly EventHandlerMediator _eventHandlerMediator;
    private readonly ICqrsEventLogger? _cqrsEventLogger;
    private readonly ConcurrentDictionary<Type, ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>>> _localEventHandlers = new();

    public IEventQueue MessageQueue { get; }

    public EventBus(
        IEventQueue messageQueue,
        EventHandlerMediator eventHandlerMediator,
        ICqrsEventLogger? cqrsEventLogger = null)
    {
        MessageQueue = messageQueue;
        _eventHandlerMediator = eventHandlerMediator;
        _cqrsEventLogger = cqrsEventLogger;
    }

    public ConcurrentDictionary<Type, ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>>> GetLocalEventHandlers()
    {
        return _localEventHandlers;
    }

    public async Task<Result> PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : EventBase
    {
        try
        {
            _cqrsEventLogger?.LogEvent(@event);

            await MessageQueue.EnqueueAsync(@event, cancellationToken);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            return Result.Success();
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

        SpinWait spinWait = new();
        while (true)
        {
            if (!_localEventHandlers.TryGetValue(typeof(TEvent), out handlers))
            {
                return Result.Success();
            }

            ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>> newBag = [.. handlers.Where(x => x.Key != handlerHash)];

            if (_localEventHandlers.TryUpdate(typeof(TEvent), newBag, handlers))
            {
                return Result.Success();
            }

            spinWait.SpinOnce();
        }
    }

    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        where TResponse : notnull
    {
        _cqrsEventLogger?.LogRequest(request);

        return await _eventHandlerMediator.SendToHanderAsync(request, cancellationToken);
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
