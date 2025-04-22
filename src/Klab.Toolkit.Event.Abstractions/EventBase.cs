using System;

namespace Klab.Toolkit.Event;

/// <summary>
/// Abstract base class for all events.
/// </summary>
public abstract record EventBase
{
    /// <summary>
    /// Id of the event
    /// </summary>
    public virtual Guid Id { get; } = Guid.NewGuid();

    /// <summary>
    /// Time of the event creation
    /// </summary>
    public virtual DateTime CreatedAt { get; } = DateTime.UtcNow;
}
