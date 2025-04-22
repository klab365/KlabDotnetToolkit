namespace Klab.Toolkit.Event.Tests;

public sealed record TestEvent1 : EventBase
{
}

public sealed record TestEvent2(string Name) : EventBase
{
}
