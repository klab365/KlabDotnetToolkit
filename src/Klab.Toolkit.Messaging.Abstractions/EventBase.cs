using System;

namespace Klab.Toolkit.Messaging;

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

    /// <summary>
    /// Gets the key used to identify duplicate deliveries of this event.
    /// </summary>
    public virtual string IdempotencyKey => Id.ToString("N");
}
