using System;
using System.Threading;
using System.Threading.Tasks;

namespace Klab.Toolkit.Messaging;

/// <summary>
/// Provides optional dead-letter support for event queues.
/// </summary>
public interface IDeadLetterEventQueue
{
    /// <summary>
    /// Stores an event that could not be processed.
    /// </summary>
    /// <param name="event">The event that could not be processed.</param>
    /// <param name="failure">The processing failure details.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeadLetterAsync(EventBase @event, EventProcessingFailure failure, CancellationToken cancellationToken = default);
}

/// <summary>
/// Describes an event processing failure.
/// </summary>
public sealed record EventProcessingFailure
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventProcessingFailure"/> class.
    /// </summary>
    /// <param name="attemptCount">The number of processing attempts.</param>
    /// <param name="exception">The exception that caused processing to fail.</param>
    public EventProcessingFailure(int attemptCount, Exception exception)
    {
        AttemptCount = attemptCount;
        Exception = exception;
    }

    /// <summary>
    /// Gets the number of processing attempts.
    /// </summary>
    public int AttemptCount { get; }

    /// <summary>
    /// Gets the exception that caused processing to fail.
    /// </summary>
    public Exception Exception { get; }
}
