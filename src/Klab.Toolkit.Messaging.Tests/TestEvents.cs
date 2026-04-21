namespace Klab.Toolkit.Messaging.Tests;

public sealed record TestEvent1 : EventBase
{
}

public sealed record TestEvent2(string Name) : EventBase
{
}
