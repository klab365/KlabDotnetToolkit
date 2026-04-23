using System.Collections.Generic;
using System.Threading;

namespace Klab.Toolkit.Messaging;

/// <summary>
/// Defines middleware that wraps the execution of a stream request handler.
/// </summary>
/// <typeparam name="TRequest">The stream request type.</typeparam>
/// <typeparam name="TResponse">The item type yielded by the stream.</typeparam>
public interface IStreamRequestMiddleware<TRequest, TResponse>
    where TRequest : IStreamRequest<TResponse>
    where TResponse : notnull
{
    /// <summary>
    /// Handles the stream request, optionally calling <paramref name="next"/> to continue the pipeline.
    /// </summary>
    IAsyncEnumerable<TResponse> HandleAsync(TRequest request, StreamHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}

/// <summary>
/// Represents the next delegate in the stream middleware pipeline.
/// </summary>
/// <typeparam name="TResponse">The item type yielded by the stream.</typeparam>
public delegate IAsyncEnumerable<TResponse> StreamHandlerDelegate<TResponse>();
