using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Klab.Toolkit.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Klab.Toolkit.Event.Tests;

public class RequestResponseEventBusTests
{
    private readonly IEventBus _eventBus;
    private readonly PingRequestHandler _pingRequestHandler;

    public RequestResponseEventBusTests()
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddRequestHandler<PingRequest, PingRequestHandler>(ServiceLifetime.Singleton);
                services.AddRequestResponseHandler<PingPongRequest, string, PingPongRequestHandler>();
                services.UseEventModule(cfg => cfg.EventQueueType = typeof(InMemoryMessageQueue));
            })
            .Build();

        _eventBus = host.Services.GetRequiredService<IEventBus>();
        _pingRequestHandler = host.Services.GetRequiredService<PingRequestHandler>();
        host.Start();
    }

    [Fact]
    public async Task SendAsync_ShouldCallPingHandler()
    {
        PingRequest req = new();

        IResult result = await _eventBus.SendAsync(req, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_ShouldCallPingPongHandler_ReturnPongMessage()
    {
        PingPongRequest req = new("Ping");

        IResult<string> result = await _eventBus.SendAsync<PingPongRequest, string>(req, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Ping Pong");
    }

    [Fact]
    public async Task SendAsync_ShouldCallAllHandlerInSpecificTime()
    {
        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < 1000; i++)
        {
            PingRequest req = new();
            await _eventBus.SendAsync(req, CancellationToken.None);
        }

        sw.Stop();
        _pingRequestHandler.Counter.Should().Be(1000);
        sw.ElapsedMilliseconds.Should().BeLessThan(100);
    }
}

internal sealed class PingRequestHandler : IRequestHandler<PingRequest>
{
    public int Counter { get; set; }

    public Task<Result> HandleAsync(PingRequest request, CancellationToken cancellationToken)
    {
        Counter++;
        return Task.FromResult(Result.Success());
    }
}

internal sealed record PingRequest : IRequest;

internal sealed class PingPongRequestHandler : IRequestHandler<PingPongRequest, string>
{
    public Task<Result<string>> HandleAsync(PingPongRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(request.Message + " Pong"));
    }
}

internal sealed record PingPongRequest(string Message) : IRequest<string>;
