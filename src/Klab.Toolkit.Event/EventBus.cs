using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    private readonly ConcurrentDictionary<Type, List<KeyValuePair<int, Func<IEvent, CancellationToken, Task<IResult>>>>> _localEventHandlers = new();

    public IEventQueue MessageQueue { get; }

    public EventBus(IEventQueue messageQueue, EventHandlerMediator eventHandlerMediator)
    {
        MessageQueue = messageQueue;
        _eventHandlerMediator = eventHandlerMediator;
    }

    public ConcurrentDictionary<Type, List<KeyValuePair<int, Func<IEvent, CancellationToken, Task<IResult>>>>> GetLocalEventHandlers()
    {
        return _localEventHandlers;
    }

    public async Task<Result> PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        await MessageQueue.EnqueueAsync(@event, cancellationToken);
        return Result.Success();
    }

    public Result Subscribe<TEvent>(Func<TEvent, CancellationToken, Task<IResult>> handler) where TEvent : IEvent
    {
        if (!_localEventHandlers.ContainsKey(typeof(TEvent)))
        {
            _localEventHandlers.TryAdd(typeof(TEvent), []);
        }

        int handlerHash = CalculateHashOfHandler(handler);
        _localEventHandlers[typeof(TEvent)].Add(new(handlerHash, (@event, token) => handler((TEvent)@event, token)));
        return Result.Success();
    }

    public Result Unsubscribe<TEvent>(Func<TEvent, CancellationToken, Task<IResult>> handler) where TEvent : IEvent
    {
        if (!_localEventHandlers.ContainsKey(typeof(TEvent)))
        {
            return Result.Failure(new InformativeError(string.Empty, "No handler found for the event type"));
        }

        int handlerHash = CalculateHashOfHandler(handler);
        _localEventHandlers[typeof(TEvent)].RemoveAll(x => x.Key == handlerHash);
        return Result.Success();
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


    private static int CalculateHashOfHandler<TEvent>(Func<TEvent, CancellationToken, Task<IResult>> handler) where TEvent : IEvent
    {
        return handler.Method.GetHashCode() ^ handler.Target?.GetHashCode() ?? throw new InvalidOperationException("Handler is not valid");
    }
}
