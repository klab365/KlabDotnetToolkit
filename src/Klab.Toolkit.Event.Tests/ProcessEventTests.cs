using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Klab.Toolkit.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Klab.Toolkit.Event.Tests;

public sealed class ProcessEventTests
{
    private readonly IEventBus _eventBus;
    private string _counter = string.Empty;

    public ProcessEventTests()
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddStreamRequestResponseHandler<SingRequest, string, SingRequestHandler>();
                services.UseEventModule();
            })
            .Build();

        _eventBus = host.Services.GetRequiredService<IEventBus>();
        host.Start();
    }

    [Fact]
    public async Task ProcessEvents_ShouldNotBlockEventHandler()
    {
        _eventBus.Subscribe<TestEvent1>(TestEvent1Handler);
        _eventBus.Subscribe<TestEvent2>(TestEvent2Handler);
        _eventBus.Subscribe<TestEvent2>(TestEvent2aHandler);

        await _eventBus.PublishAsync(new TestEvent2("Event2a"), CancellationToken.None);
        await _eventBus.PublishAsync(new TestEvent2("Event2b"), CancellationToken.None);
        await _eventBus.PublishAsync(new TestEvent1(), CancellationToken.None);
        await Task.Delay(5000);

        _counter.Should().Be("2a2a1");
    }

    private Task<Result> TestEvent2aHandler(TestEvent2 @event, CancellationToken token)
    {
        _counter += "a";
        return Task.FromResult<Result>(Result.Success());
    }

    private Task<Result> TestEvent2Handler(TestEvent2 @event, CancellationToken token)
    {
        _counter += "2";
        return Task.FromResult<Result>(Result.Success());
    }

    private Task<Result> TestEvent1Handler(TestEvent1 @event, CancellationToken token)
    {
        _counter += "1";
        return Task.FromResult<Result>(Result.Success());
    }
}
