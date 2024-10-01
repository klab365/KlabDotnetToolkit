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
    ConcurrentDictionary<Type, List<KeyValuePair<Guid, Func<IEvent, CancellationToken, Task<IResult>>>>> GetLocalEventHandlers();

    /// <summary>
    /// Publishes an event and not wait until event is processed.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result> PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;

    /// <summary>
    /// Subscribes to an event to a local function
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="handler"></param>
    /// <returns></returns>
    Result<Guid> Subscribe<TEvent>(Func<TEvent, CancellationToken, Task<IResult>> handler) where TEvent : IEvent;

    /// <summary>
    /// Unsubscribes from an event
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="id"></param>
    /// <returns></returns>
    Result Unsuscribe<TEvent>(Guid id) where TEvent : IEvent;

    /// <summary>
    /// Sends a request and waits for the response
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IResult<TResponse>> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest<TResponse>
        where TResponse : notnull;

    /// <summary>
    /// Sends a request and waits for the response
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IResult> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest;
}
