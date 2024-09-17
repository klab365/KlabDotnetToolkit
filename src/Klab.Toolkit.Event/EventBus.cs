using System;
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
    public IEventQueue MessageQueue { get; }

    public Dictionary<Type, List<KeyValuePair<Guid, Func<IEvent, CancellationToken, Task>>>> LocalEventHandlers { get; } = new();

    public EventBus(IEventQueue messageQueue)
    {
        MessageQueue = messageQueue;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        await MessageQueue.EnqueueAsync(@event, cancellationToken);
    }

    public Result<Guid> Subscribe<TEvent>(Func<TEvent, CancellationToken, Task> handler) where TEvent : IEvent
    {
        if (!LocalEventHandlers.ContainsKey(typeof(TEvent)))
        {
            LocalEventHandlers.Add(typeof(TEvent), new());
        }

        Guid id = Guid.NewGuid();
        LocalEventHandlers[typeof(TEvent)].Add(new(id, (@event, token) => handler((TEvent)@event, token)));
        return Result.Success(id);
    }

    public Result Unsuscribe<TEvent>(Guid id) where TEvent : IEvent
    {
        if (!LocalEventHandlers.ContainsKey(typeof(TEvent)))
        {
            return Result.Success();
        }

        LocalEventHandlers[typeof(TEvent)].RemoveAll(x => x.Key == id);
        return Result.Success();
    }
}
