using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.Messaging;

internal abstract class StreamRequestResponseHandlerWrapper
{
    public abstract IAsyncEnumerable<object> HandleAsync(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

internal class StreamRequestResponseHandlerWrapper<TRequest, TResponse> : StreamRequestResponseHandlerWrapper
    where TRequest : IStreamRequest<TResponse>
    where TResponse : notnull
{
    public override async IAsyncEnumerable<object> HandleAsync(object request, IServiceProvider serviceProvider, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (request is not TRequest castedReq)
        {
            throw new InvalidOperationException($"Request type mismatch. Expected {typeof(TRequest).Name} but received {request.GetType().Name}");
        }

        IStreamRequestHandler<TRequest, TResponse> handler = serviceProvider.GetRequiredService<IStreamRequestHandler<TRequest, TResponse>>();
        IEnumerable<IStreamRequestMiddleware<TRequest, TResponse>> middlewares = serviceProvider.GetServices<IStreamRequestMiddleware<TRequest, TResponse>>();

        StreamHandlerDelegate<TResponse> pipeline = () => handler.HandleAsync(castedReq, cancellationToken);

        foreach (IStreamRequestMiddleware<TRequest, TResponse> middleware in middlewares.Reverse())
        {
            StreamHandlerDelegate<TResponse> next = pipeline;
            IStreamRequestMiddleware<TRequest, TResponse> current = middleware;
            pipeline = () => current.HandleAsync(castedReq, next, cancellationToken);
        }

        await foreach (TResponse item in pipeline())
        {
            yield return item;
        }
    }
}
