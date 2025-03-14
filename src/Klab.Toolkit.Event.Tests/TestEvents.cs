using System;

namespace Klab.Toolkit.Event.Tests;

public sealed record TestEvent1 : IEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

}

public sealed record TestEvent2(string Name) : IEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public string Name { get; init; } = Name;
}
