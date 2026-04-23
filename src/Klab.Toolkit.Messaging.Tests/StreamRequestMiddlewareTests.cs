using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Klab.Toolkit.Messaging.Tests;

public class StreamRequestMiddlewareTests
{
    [Fact]
    public async Task Stream_WithSingleMiddleware_MiddlewareIsInvoked()
    {
        CallTracker tracker = new();

        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(tracker);
                services.AddStreamRequestResponseHandler<SingRequest, string, SingRequestHandler>();
                services.AddStreamRequestMiddleware<SingRequest, string, TrackingStreamMiddleware>();
                services.AddMessagingModule();
            })
            .Build();

        IMediator mediator = host.Services.GetRequiredService<IMediator>();
        host.Start();

        await foreach (string _ in mediator.Stream(new SingRequest(), CancellationToken.None))
        { }

        tracker.Calls.Should().ContainSingle().Which.Should().Be(nameof(TrackingStreamMiddleware));
    }

    [Fact]
    public async Task Stream_WithMultipleMiddlewares_AllInvokedInRegistrationOrder()
    {
        CallTracker tracker = new();

        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(tracker);
                services.AddStreamRequestResponseHandler<SingRequest, string, SingRequestHandler>();
                services.AddStreamRequestMiddleware<SingRequest, string, FirstStreamMiddleware>();
                services.AddStreamRequestMiddleware<SingRequest, string, SecondStreamMiddleware>();
                services.AddMessagingModule();
            })
            .Build();

        IMediator mediator = host.Services.GetRequiredService<IMediator>();
        host.Start();

        await foreach (string _ in mediator.Stream(new SingRequest(), CancellationToken.None))
        { }

        tracker.Calls.Should().Equal(nameof(FirstStreamMiddleware), nameof(SecondStreamMiddleware));
    }

    [Fact]
    public async Task Stream_MiddlewareCanTransformItems()
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddStreamRequestResponseHandler<SingRequest, string, SingRequestHandler>();
                services.AddStreamRequestMiddleware<SingRequest, string, UpperCaseStreamMiddleware>();
                services.AddMessagingModule();
            })
            .Build();

        IMediator mediator = host.Services.GetRequiredService<IMediator>();
        host.Start();

        List<string> results = new();
        await foreach (string item in mediator.Stream(new SingRequest(), CancellationToken.None))
        {
            results.Add(item);
        }

        results.Should().Equal("SING1", "SING2", "SING3");
    }

    [Fact]
    public async Task Stream_WithNoMiddleware_HandlerItemsReturnedDirectly()
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddStreamRequestResponseHandler<SingRequest, string, SingRequestHandler>();
                services.AddMessagingModule();
            })
            .Build();

        IMediator mediator = host.Services.GetRequiredService<IMediator>();
        host.Start();

        List<string> results = new();
        await foreach (string item in mediator.Stream(new SingRequest(), CancellationToken.None))
        {
            results.Add(item);
        }

        results.Should().Equal("Sing1", "Sing2", "Sing3");
    }
}

internal sealed class TrackingStreamMiddleware : IStreamRequestMiddleware<SingRequest, string>
{
    private readonly CallTracker _tracker;

    public TrackingStreamMiddleware(CallTracker tracker)
    {
        _tracker = tracker;
    }

    public async IAsyncEnumerable<string> HandleAsync(SingRequest request, StreamHandlerDelegate<string> next, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _tracker.Calls.Add(nameof(TrackingStreamMiddleware));
        await foreach (string item in next().WithCancellation(cancellationToken))
        {
            yield return item;
        }
    }
}

internal sealed class FirstStreamMiddleware : IStreamRequestMiddleware<SingRequest, string>
{
    private readonly CallTracker _tracker;

    public FirstStreamMiddleware(CallTracker tracker)
    {
        _tracker = tracker;
    }

    public async IAsyncEnumerable<string> HandleAsync(SingRequest request, StreamHandlerDelegate<string> next, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _tracker.Calls.Add(nameof(FirstStreamMiddleware));
        await foreach (string item in next().WithCancellation(cancellationToken))
        {
            yield return item;
        }
    }
}

internal sealed class SecondStreamMiddleware : IStreamRequestMiddleware<SingRequest, string>
{
    private readonly CallTracker _tracker;

    public SecondStreamMiddleware(CallTracker tracker)
    {
        _tracker = tracker;
    }

    public async IAsyncEnumerable<string> HandleAsync(SingRequest request, StreamHandlerDelegate<string> next, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _tracker.Calls.Add(nameof(SecondStreamMiddleware));
        await foreach (string item in next().WithCancellation(cancellationToken))
        {
            yield return item;
        }
    }
}

internal sealed class UpperCaseStreamMiddleware : IStreamRequestMiddleware<SingRequest, string>
{
    public async IAsyncEnumerable<string> HandleAsync(SingRequest request, StreamHandlerDelegate<string> next, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (string item in next().WithCancellation(cancellationToken))
        {
            yield return item.ToUpperInvariant();
        }
    }
}
