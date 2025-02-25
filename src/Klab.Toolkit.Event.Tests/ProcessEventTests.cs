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
    private readonly object _lock = new();
    private string _counter = string.Empty;
    private TaskCompletionSource<bool> _tcs = new();

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

        await _eventBus.PublishAsync(new TestEvent2(), CancellationToken.None);
        await _eventBus.PublishAsync(new TestEvent2(), CancellationToken.None);
        await _eventBus.PublishAsync(new TestEvent1(), CancellationToken.None);
        await Task.Delay(1000);

        _counter.Should().Be("122");
    }

    private async Task<IResult> TestEvent2Handler(TestEvent2 @event, CancellationToken token)
    {
        await _tcs.Task;

        lock (_lock)
        {
            _counter += "2";
        }

        return Result.Success();
    }

    private Task<IResult> TestEvent1Handler(TestEvent1 @event, CancellationToken token)
    {
        lock (_lock)
        {
            _counter += "1";
        }

        _tcs.SetResult(true);
        return Task.FromResult<IResult>(Result.Success());
    }
}
