using System;
using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.Messaging;

/// <summary>
/// Messaging Module Configuration
/// </summary>
public class MessagingModuleConfiguration
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
    /// Gets or sets the messaging logger type
    /// Defaults to NullMessagingLogger which does not log anything. You can set it to a custom implementation of IMessagingLogger to enable logging.
    /// </summary>
    public Type? MessagingLoggerType { get; set; } = typeof(NullMessagingLogger);

    /// <summary>
    /// Gets or sets the messaging logger path
    /// </summary>
    public string MessagingLoggerPath { get; set; } = "messaging-logs.json";
}
