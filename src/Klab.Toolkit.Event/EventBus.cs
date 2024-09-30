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
    private readonly ConcurrentDictionary<Type, List<KeyValuePair<Guid, Func<IEvent, CancellationToken, Task>>>> _localEventHandlers = new();

    public IEventQueue MessageQueue { get; }

    public ConcurrentDictionary<Type, List<KeyValuePair<Guid, Func<IEvent, CancellationToken, Task>>>> GetLocalEventHandlers()
    {
        return _localEventHandlers;
    }

    public EventBus(IEventQueue messageQueue, EventHandlerMediator eventHandlerMediator)
    {
        MessageQueue = messageQueue;
        _eventHandlerMediator = eventHandlerMediator;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        await MessageQueue.EnqueueAsync(@event, cancellationToken);
    }

    public Result<Guid> Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : IEvent
    {
        if (!_localEventHandlers.ContainsKey(typeof(TEvent)))
        {
            _localEventHandlers.TryAdd(typeof(TEvent), []);
        }

        Guid id = Guid.NewGuid();
        _localEventHandlers[typeof(TEvent)].Add(new(id, (@event, token) => handler((TEvent)@event, token)));
        return Result.Success(id);
    }

    public Result Unsuscribe<TEvent>(Guid id) where TEvent : IEvent
    {
        if (!_localEventHandlers.ContainsKey(typeof(TEvent)))
        {
            return Result.Failure(new InformativeError(string.Empty, "No handler found for the event type"));
        }

        _localEventHandlers[typeof(TEvent)].RemoveAll(x => x.Key == id);
        return Result.Success();
    }

    public Task<Result<TResponse>> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
        where TResponse : notnull
    {
        return _eventHandlerMediator.SendToHanderAsync<TRequest, TResponse>(request, cancellationToken);
    }

    public Task<Result> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        return _eventHandlerMediator.SendToHanderAsync(request, cancellationToken);
    }
}
