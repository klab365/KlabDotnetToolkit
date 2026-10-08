using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Klab.Toolkit.Messaging;

internal interface IEventHandlerWrapperRegistration
{
    Type EventType { get; }
    IEnumerable<EventHandlerExecutor> GetHandlers(IServiceProvider services, EventBase message);
}

internal sealed class EventHandlerWrapperRegistration<TEvent> : IEventHandlerWrapperRegistration where TEvent : EventBase
{
    private readonly IServiceProvider _services;
    private readonly EventHandlerWrapper<TEvent> _wrapper;

    public EventHandlerWrapperRegistration(IServiceProvider services)
    {
        _services = services;
        _wrapper = new EventHandlerWrapper<TEvent>();
    }

    public Type EventType => typeof(TEvent);

    public IEnumerable<EventHandlerExecutor> GetHandlers(IServiceProvider services, EventBase message)
    {
        if (message is not TEvent)
        {
            throw new InvalidOperationException($"Event type mismatch. Expected {typeof(TEvent).Name} but received {message.GetType().Name}");
        }

        return _wrapper.GetHandlers(services);
    }
}

internal interface IRequestResponseHandlerWrapperRegistration
{
    Type RequestType { get; }
    Type ResponseType { get; }
    Task<object> HandleAsync(object request, IServiceProvider services, CancellationToken cancellationToken);
}

internal sealed class RequestResponseHandlerWrapperRegistration<TRequest, TResponse> : IRequestResponseHandlerWrapperRegistration
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    private readonly RequestResponseHandlerWrapper<TRequest, TResponse> _wrapper = new();

    public RequestResponseHandlerWrapperRegistration(IServiceProvider services)
    {
    }

    public Type RequestType => typeof(TRequest);
    public Type ResponseType => typeof(TResponse);

    public Task<object> HandleAsync(object request, IServiceProvider services, CancellationToken cancellationToken)
    {
        return _wrapper.HandleAsync(request, services, cancellationToken);
    }
}

internal interface IStreamRequestResponseHandlerWrapperRegistration
{
    Type RequestType { get; }
    Type ResponseType { get; }
    IAsyncEnumerable<object> HandleAsync(object request, IServiceProvider services, CancellationToken cancellationToken);
}

internal sealed class StreamRequestResponseHandlerWrapperRegistration<TRequest, TResponse> : IStreamRequestResponseHandlerWrapperRegistration
    where TRequest : IStreamRequest<TResponse>
    where TResponse : notnull
{
    private readonly StreamRequestResponseHandlerWrapper<TRequest, TResponse> _wrapper = new();

    public StreamRequestResponseHandlerWrapperRegistration(IServiceProvider services)
    {
    }

    public Type RequestType => typeof(TRequest);
    public Type ResponseType => typeof(TResponse);

    public IAsyncEnumerable<object> HandleAsync(object request, IServiceProvider services, CancellationToken cancellationToken)
    {
        return _wrapper.HandleAsync(request, services, cancellationToken);
    }
}
