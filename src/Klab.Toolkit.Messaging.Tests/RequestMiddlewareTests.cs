using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Klab.Toolkit.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Klab.Toolkit.Messaging.Tests;

public class RequestMiddlewareTests
{
    [Fact]
    public async Task SendAsync_WithSingleMiddleware_MiddlewareIsInvoked()
    {
        CallTracker tracker = new();

        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(tracker);
                services.AddRequestResponseHandler<PingRequest, Result, PingRequestHandler>(ServiceLifetime.Singleton);
                services.AddRequestMiddleware<PingRequest, Result, TrackingMiddleware>();
                services.AddMessagingModule();
            })
            .Build();

        IMediator mediator = host.Services.GetRequiredService<IMediator>();
        host.Start();

        await mediator.SendAsync(new PingRequest(), CancellationToken.None);

        tracker.Calls.Should().ContainSingle().Which.Should().Be(nameof(TrackingMiddleware));
    }

    [Fact]
    public async Task SendAsync_WithMultipleMiddlewares_AllInvokedInRegistrationOrder()
    {
        CallTracker tracker = new();

        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(tracker);
                services.AddRequestResponseHandler<PingRequest, Result, PingRequestHandler>(ServiceLifetime.Singleton);
                services.AddRequestMiddleware<PingRequest, Result, FirstMiddleware>();
                services.AddRequestMiddleware<PingRequest, Result, SecondMiddleware>();
                services.AddMessagingModule();
            })
            .Build();

        IMediator mediator = host.Services.GetRequiredService<IMediator>();
        host.Start();

        await mediator.SendAsync(new PingRequest(), CancellationToken.None);

        tracker.Calls.Should().Equal(nameof(FirstMiddleware), nameof(SecondMiddleware));
    }

    [Fact]
    public async Task SendAsync_MiddlewareCanShortCircuit_HandlerNotCalled()
    {
        CallTracker tracker = new();

        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(tracker);
                services.AddRequestResponseHandler<PingRequest, Result, CountingRequestHandler>(ServiceLifetime.Singleton);
                services.AddRequestMiddleware<PingRequest, Result, ShortCircuitMiddleware>();
                services.AddMessagingModule();
            })
            .Build();

        IMediator mediator = host.Services.GetRequiredService<IMediator>();
        CountingRequestHandler handler = host.Services.GetRequiredService<CountingRequestHandler>();
        host.Start();

        Result result = await mediator.SendAsync(new PingRequest(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        handler.Counter.Should().Be(0);
    }

    [Fact]
    public async Task SendAsync_WithNoMiddleware_HandlerCalledDirectly()
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddRequestResponseHandler<PingRequest, Result, CountingRequestHandler>(ServiceLifetime.Singleton);
                services.AddMessagingModule();
            })
            .Build();

        IMediator mediator = host.Services.GetRequiredService<IMediator>();
        CountingRequestHandler handler = host.Services.GetRequiredService<CountingRequestHandler>();
        host.Start();

        await mediator.SendAsync(new PingRequest(), CancellationToken.None);

        handler.Counter.Should().Be(1);
    }
}

internal sealed class CallTracker
{
    public List<string> Calls { get; } = new();
}

internal sealed class TrackingMiddleware : IRequestMiddleware<PingRequest, Result>
{
    private readonly CallTracker _tracker;

    public TrackingMiddleware(CallTracker tracker)
    {
        _tracker = tracker;
    }

    public async Task<Result> HandleAsync(PingRequest request, RequestHandlerDelegate<Result> next, CancellationToken cancellationToken)
    {
        _tracker.Calls.Add(nameof(TrackingMiddleware));
        return await next();
    }
}

internal sealed class FirstMiddleware : IRequestMiddleware<PingRequest, Result>
{
    private readonly CallTracker _tracker;

    public FirstMiddleware(CallTracker tracker)
    {
        _tracker = tracker;
    }

    public async Task<Result> HandleAsync(PingRequest request, RequestHandlerDelegate<Result> next, CancellationToken cancellationToken)
    {
        _tracker.Calls.Add(nameof(FirstMiddleware));
        return await next();
    }
}

internal sealed class SecondMiddleware : IRequestMiddleware<PingRequest, Result>
{
    private readonly CallTracker _tracker;

    public SecondMiddleware(CallTracker tracker)
    {
        _tracker = tracker;
    }

    public async Task<Result> HandleAsync(PingRequest request, RequestHandlerDelegate<Result> next, CancellationToken cancellationToken)
    {
        _tracker.Calls.Add(nameof(SecondMiddleware));
        return await next();
    }
}

internal sealed class ShortCircuitMiddleware : IRequestMiddleware<PingRequest, Result>
{
    public Task<Result> HandleAsync(PingRequest request, RequestHandlerDelegate<Result> next, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success());
    }
}

internal sealed class CountingRequestHandler : IRequestHandler<PingRequest, Result>
{
    public int Counter { get; private set; }

    public Task<Result> HandleAsync(PingRequest request, CancellationToken cancellationToken)
    {
        Counter++;
        return Task.FromResult(Result.Success());
    }
}
