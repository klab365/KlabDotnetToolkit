using System;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;

namespace Klab.Toolkit.Event;

/// <summary>
/// Interface for logging events and commands in the event bus.
/// </summary>
public interface IEventBusLogger
{
    /// <summary>
    /// Log an event that was processed.
    /// </summary>
    /// <param name="event">The event that was processed.</param>
    /// <param name="handlerResults">The results from the event handlers.</param>
    ValueTask LogEventAsync(EventBase @event, Result[] handlerResults);

    /// <summary>
    /// Log a command/request that was executed.
    /// </summary>
    /// <param name="requestType">The type of the request.</param>
    /// <param name="requestData">The request data.</param>
    /// <param name="response">The response from the handler, or null if void.</param>
    ValueTask LogCommandAsync(Type requestType, object requestData, object? response);

    /// <summary>
    /// Log a stream request that was executed.
    /// </summary>
    /// <param name="requestType">The type of the stream request.</param>
    /// <param name="requestData">The data of the stream request.</param>
    /// <param name="response">The response from the handler, or null if void.</param>
    ValueTask LogStreamRequestAsync(Type requestType, object requestData, object? response);

    /// <summary>
    /// Waits until all currently enqueued log entries have been persisted.
    /// </summary>
    Task FlushAsync(CancellationToken cancellationToken = default);
}
