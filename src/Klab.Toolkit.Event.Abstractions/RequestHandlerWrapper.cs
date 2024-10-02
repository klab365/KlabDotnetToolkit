using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.Event;

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

        TResponse res = await handler.HandleAsync(castedReq, cancellationToken);
        if (res is not TResponse castedResp)
        {
            throw new InvalidOperationException($"Response type mismatch. Expected {typeof(TResponse).Name} but received {res.GetType().Name}");
        }

        return castedResp;
    }
}
