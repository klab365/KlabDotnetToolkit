using System.Threading;
using System.Threading.Tasks;

namespace Klab.Toolkit.Messaging;

/// <summary>
/// Defines middleware that wraps the execution of a request handler.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IRequestMiddleware<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    /// <summary>
    /// Handles the request, optionally calling <paramref name="next"/> to continue the pipeline.
    /// </summary>
    Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}

/// <summary>
/// Represents the next delegate in the middleware pipeline.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
