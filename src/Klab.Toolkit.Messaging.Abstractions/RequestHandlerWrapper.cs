using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.Messaging;

internal abstract class RequestResponseHandlerWrapper
{
    public abstract Task<object> HandleAsync(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

internal class RequestResponseHandlerWrapper<TRequest, TResponse> : RequestResponseHandlerWrapper
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public override async Task<object> HandleAsync(object request, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        if (request is not TRequest castedReq)
        {
            throw new InvalidOperationException($"Request type mismatch. Expected {typeof(TRequest).Name} but received {request.GetType().Name}");
        }

        IRequestHandler<TRequest, TResponse> handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        IEnumerable<IRequestMiddleware<TRequest, TResponse>> middlewares = serviceProvider.GetServices<IRequestMiddleware<TRequest, TResponse>>();

        RequestHandlerDelegate<TResponse> pipeline = () => handler.HandleAsync(castedReq, cancellationToken);

        foreach (IRequestMiddleware<TRequest, TResponse> middleware in middlewares.Reverse())
        {
            RequestHandlerDelegate<TResponse> next = pipeline;
            IRequestMiddleware<TRequest, TResponse> current = middleware;
            pipeline = () => current.HandleAsync(castedReq, next, cancellationToken);
        }

        return await pipeline();
    }
}
