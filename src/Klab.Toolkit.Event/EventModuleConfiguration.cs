using System;
using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.Event;

/// <summary>
/// Event Module Configuration
/// </summary>
public class EventModuleConfiguration
{
    /// <summary>
    /// Gets or sets the event queue type
    /// </summary>
    public Type? EventQueueType { get; set; } = typeof(InMemoryMessageQueue);

    /// <summary>
    /// Gets or sets the event queue lifetime
    /// </summary>
    public ServiceLifetime EventQueueLifetime { get; set; } = ServiceLifetime.Singleton;

    /// <summary>
    /// Gets or sets the event bus logger type
    /// Defaults to NullEventBusLogger which does not log anything. You can set it to a custom implementation of IEventBusLogger to enable logging.
    /// </summary>
    public Type? EventBusLoggerType { get; set; } = typeof(NullEventBusLogger);

    /// <summary>
    /// Gets or sets the event bus logger path
    /// </summary>
    public string EventBusLoggerPath { get; set; } = "event-logs.json";
}
