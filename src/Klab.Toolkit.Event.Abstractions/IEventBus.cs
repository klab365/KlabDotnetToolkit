using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;

namespace Klab.Toolkit.Event;

/// <summary>
/// Represents a message bus
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Getter property for the message queue
    /// </summary>
    IEventQueue MessageQueue { get; }

    /// <summary>
    /// Getter property for the local event handlers
    /// </summary>
    ConcurrentDictionary<Type, ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>>> GetLocalEventHandlers();

    /// <summary>
    /// Publishes an event and not wait until event is processed.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result> PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : EventBase;

    /// <summary>
    /// Subscribes to an event to a local function
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="handler"></param>
    /// <returns></returns>
    Result Subscribe<TEvent>(Func<TEvent, CancellationToken, Task<Result>> handler) where TEvent : EventBase;

    /// <summary>
    /// Unsubscribes from an event with the local function
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="handler"></param>
    /// <returns></returns>
    Result Unsubscribe<TEvent>(Func<TEvent, CancellationToken, Task<Result>> handler) where TEvent : EventBase;

    /// <summary>
    /// Sends a request and waits for the response
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        where TResponse : notnull;

    /// <summary>
    /// Streams a request and returns an async enumerable
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IAsyncEnumerable<TResponse> Stream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        where TResponse : notnull;
}
