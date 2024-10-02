using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.Event;

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

        await foreach (TResponse item in handler.HandleAsync(castedReq, cancellationToken))
        {
            yield return item;
        }
    }
}
