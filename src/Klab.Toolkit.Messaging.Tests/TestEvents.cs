namespace Klab.Toolkit.Messaging.Tests;

public sealed record TestEvent1 : EventBase, IRecordable
{
    public string ToRecordingDataJson() => "{}";
}

public sealed record TestEvent2(string Name) : EventBase, IRecordable
{
    public string ToRecordingDataJson() => $"{{\"Name\":\"{Name}\"}}";
}
