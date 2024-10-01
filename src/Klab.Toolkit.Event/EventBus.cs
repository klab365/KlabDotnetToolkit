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
    private readonly ConcurrentDictionary<Type, List<KeyValuePair<Guid, Func<IEvent, CancellationToken, Task<IResult>>>>> _localEventHandlers = new();

    public IEventQueue MessageQueue { get; }

    public EventBus(IEventQueue messageQueue, EventHandlerMediator eventHandlerMediator)
    {
        MessageQueue = messageQueue;
        _eventHandlerMediator = eventHandlerMediator;
    }

    public ConcurrentDictionary<Type, List<KeyValuePair<Guid, Func<IEvent, CancellationToken, Task<IResult>>>>> GetLocalEventHandlers()
    {
        return _localEventHandlers;
    }

    public async Task<Result> PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        await MessageQueue.EnqueueAsync(@event, cancellationToken);
        return Result.Success();
    }

    public Result<Guid> Subscribe<TEvent>(Func<TEvent, CancellationToken, Task<IResult>> handler) where TEvent : IEvent
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

    public Task<IResult<TResponse>> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
        where TResponse : notnull
    {
        return _eventHandlerMediator.SendToHanderAsync<TRequest, TResponse>(request, cancellationToken);
    }

    public Task<IResult> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        return _eventHandlerMediator.SendToHanderAsync(request, cancellationToken);
    }
}
