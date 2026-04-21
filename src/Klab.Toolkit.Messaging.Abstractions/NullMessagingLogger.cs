using System;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;

namespace Klab.Toolkit.Messaging;

/// <summary>
/// No-op implementation of IMessagingLogger.
/// </summary>
public sealed class NullMessagingLogger : IMessagingLogger
{
    /// <inheritdoc/>
    public ValueTask LogEventAsync(EventBase @event, Result[] handlerResults) => default;

    /// <inheritdoc/>
    public ValueTask LogCommandAsync(Type requestType, object requestData, object? response) => default;

    /// <inheritdoc/>
    public ValueTask LogStreamRequestAsync(Type requestType, object requestData, object? response) => default;

    /// <inheritdoc/>
    public Task FlushAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
