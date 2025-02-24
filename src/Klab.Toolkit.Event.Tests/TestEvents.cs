using System;

namespace Klab.Toolkit.Event.Tests;

internal sealed record TestEvent1 : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}

internal sealed record TestEvent2 : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
