using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;

namespace Klab.Toolkit.Messaging;

/// <summary>
/// Basic implementation of <see cref="IMediator"/>
/// </summary>
internal sealed class Mediator : IMediator
{
    private readonly MessagingHandlerMediator _eventHandlerMediator;
    private readonly ConcurrentDictionary<Type, ConcurrentBag<LocalHandlerEntry>> _localEventHandlers = new();

    public IEventQueue MessageQueue { get; }

    public Mediator(IEventQueue messageQueue, MessagingHandlerMediator eventHandlerMediator)
    {
        MessageQueue = messageQueue;
        _eventHandlerMediator = eventHandlerMediator;
    }

    public ConcurrentDictionary<Type, ConcurrentBag<Func<EventBase, CancellationToken, Task<Result>>>> GetLocalEventHandlers()
    {
        ConcurrentDictionary<Type, ConcurrentBag<Func<EventBase, CancellationToken, Task<Result>>>> result = new();
        foreach (System.Collections.Generic.KeyValuePair<Type, ConcurrentBag<LocalHandlerEntry>> kvp in _localEventHandlers)
        {
            result[kvp.Key] = [.. kvp.Value.Select(e => e.Wrapper)];
        }

        return result;
    }

    internal bool TryGetWrappers(Type eventType, out IEnumerable<Func<EventBase, CancellationToken, Task<Result>>> wrappers)
    {
        if (_localEventHandlers.TryGetValue(eventType, out ConcurrentBag<LocalHandlerEntry>? entries))
        {
            wrappers = entries.Select(e => e.Wrapper);
            return true;
        }

        wrappers = [];
        return false;
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
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.Create("Mediator.PublishAsync", ex.Message));
        }
    }

    public Result Subscribe<TEvent>(Func<TEvent, CancellationToken, Task<Result>> handler) where TEvent : EventBase
    {
        ConcurrentBag<LocalHandlerEntry> entries = _localEventHandlers.GetOrAdd(typeof(TEvent), _ => []);

        if (entries.Any(e => e.Original.Equals(handler)))
        {
            return Result.Failure(Error.Create(string.Empty, "Handler already exists"));
        }

        entries.Add(new LocalHandlerEntry(handler, (@event, token) => handler((TEvent)@event, token)));
        return Result.Success();
    }

    public Result Unsubscribe<TEvent>(Func<TEvent, CancellationToken, Task<Result>> handler) where TEvent : EventBase
    {
        if (!_localEventHandlers.TryGetValue(typeof(TEvent), out ConcurrentBag<LocalHandlerEntry>? entries))
        {
            return Result.Failure(Error.Create(string.Empty, "No handler found for the event type"));
        }

        SpinWait spinWait = new();
        while (true)
        {
            if (!_localEventHandlers.TryGetValue(typeof(TEvent), out entries))
            {
                return Result.Success();
            }

            ConcurrentBag<LocalHandlerEntry> newBag = [.. entries.Where(e => !e.Original.Equals(handler))];

            if (_localEventHandlers.TryUpdate(typeof(TEvent), newBag, entries))
            {
                return Result.Success();
            }

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

    private sealed class LocalHandlerEntry
    {
        public Delegate Original { get; }
        public Func<EventBase, CancellationToken, Task<Result>> Wrapper { get; }

        public LocalHandlerEntry(Delegate original, Func<EventBase, CancellationToken, Task<Result>> wrapper)
        {
            Original = original;
            Wrapper = wrapper;
        }
    }
}
