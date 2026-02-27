using System.Collections.Generic;
using Klab.Toolkit.Results;

namespace Klab.Toolkit.Event;

/// <summary>
/// Interface for logging CQRs events and requests
/// </summary>
public interface ICqrsEventLogger
{
    /// <summary>
    /// Log a published event (before processing)
    /// </summary>
    void LogEvent(EventBase @event);

    /// <summary>
    /// Log a sent request
    /// </summary>
    void LogRequest<T>(IRequest<T> request);

    /// <summary>
    /// Log a processed event with results
    /// </summary>
    void LogProcessedEvent(EventBase @event, IEnumerable<Result> results);
}
