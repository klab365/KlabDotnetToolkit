using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;

namespace Klab.Toolkit.Event;

internal sealed class EventBus : IEventBus
{
    private readonly EventHandlerMediator _eventHandlerMediator;
    private readonly EventModuleConfiguration _configuration;
    private readonly ConcurrentDictionary<Type, ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>>> _localEventHandlers = new();
    private readonly List<object> _eventLogs = new();
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public IEventQueue MessageQueue { get; }

    public EventBus(
        IEventQueue messageQueue,
        EventHandlerMediator eventHandlerMediator,
        EventModuleConfiguration configuration)
    {
        MessageQueue = messageQueue;
        _eventHandlerMediator = eventHandlerMediator;
        _configuration = configuration;
        _jsonSerializerOptions = new()
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() },
            PropertyNameCaseInsensitive = true,
        };
        _jsonSerializerOptions.Converters.Add(new EventInterfaceJsonConverter());
    }

    public ConcurrentDictionary<Type, ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>>> GetLocalEventHandlers()
    {
        return _localEventHandlers;
    }

    public async Task<Result> PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : EventBase
    {
        try
        {
            if (_configuration.ShouldLogEvents)
            {
                LogPublishedEvent(@event);
            }

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

    private void LogPublishedEvent(EventBase @event)
    {
        object eventLog = new { Event = @event, Stage = "Published" };
        _eventLogs.Add(eventLog);
        string jsonLog = JsonSerializer.Serialize(_eventLogs, _jsonSerializerOptions);
        File.WriteAllText(_configuration.EventLogFilePath, jsonLog);
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
        if (_configuration.ShouldLogEvents)
        {
            LogSentRequest(request);
        }

        return await _eventHandlerMediator.SendToHanderAsync(request, cancellationToken);
    }

    private void LogSentRequest<T>(IRequest<T> request)
    {
        object requestLog = new { Request = request, Stage = "Sent" };
        _eventLogs.Add(requestLog);
        string jsonLog = JsonSerializer.Serialize(_eventLogs, _jsonSerializerOptions);
        File.WriteAllText(_configuration.EventLogFilePath, jsonLog);
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
