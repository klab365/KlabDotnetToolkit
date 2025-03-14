using System;

namespace Klab.Toolkit.Event;

/// <summary>
/// Marker interface for events
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Id of the event
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Time of the event creation
    /// </summary>
    DateTimeOffset CreatedAt { get; }
}
