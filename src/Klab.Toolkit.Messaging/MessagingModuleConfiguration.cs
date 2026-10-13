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

    /// <summary>
    /// Gets or sets the maximum number of retries after the initial processing attempt.
    /// </summary>
    public int EventProcessingRetryCount { get; set; }

    /// <summary>
    /// Gets or sets the delay before the first retry.
    /// </summary>
    public TimeSpan EventProcessingRetryDelay { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Gets or sets the multiplier applied to the retry delay after each failed attempt.
    /// </summary>
    public double EventProcessingRetryBackoffFactor { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum time allowed for one event processing attempt.
    /// A value of <see langword="null"/> disables the timeout.
    /// </summary>
    public TimeSpan? EventProcessingTimeout { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether successfully processed idempotency keys are retained to prevent duplicate delivery.
    /// </summary>
    public bool EnableIdempotency { get; set; } = true;
}
