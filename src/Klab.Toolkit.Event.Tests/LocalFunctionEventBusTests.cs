using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Klab.Toolkit.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Klab.Toolkit.Event.Tests;

public class LocalFunctionEventBusTests
{
    private readonly IEventBus _eventBus;
    private int _counter;

    public LocalFunctionEventBusTests()
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.UseEventModule(cfg =>
                {
                    cfg.EventQueueType = typeof(InMemoryMessageQueue);
                });
            })
            .Build();

        _eventBus = host.Services.GetRequiredService<IEventBus>();
        host.Start();
    }

    [Fact]
    public async Task SimpleEventPublishTest()
    {
        // arrange
        Result<Guid> id = _eventBus.Subscribe<TestEvent>(IncreaseCounterAsync);

        // act
        await _eventBus.PublishAsync(new TestEvent());
        await Task.Delay(1000); // to be processed

        // assert
        _counter.Should().Be(1);
        id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task SubscribeUnsubscribeTest()
    {
        // arrange
        Result<Guid> id = _eventBus.Subscribe<TestEvent>(IncreaseCounterAsync);

        // act
        await _eventBus.PublishAsync(new TestEvent());
        await Task.Delay(1000); // to be processed

        // assert
        _counter.Should().Be(1);

        // act
        _eventBus.Unsuscribe<TestEvent>(id.Value);
        await _eventBus.PublishAsync(new TestEvent());
        await Task.Delay(1000); // to be processed

        // assert
        _counter.Should().Be(1);
    }

    private Task<IResult> IncreaseCounterAsync(TestEvent @event, CancellationToken cancellationToken)
    {
        _counter++;
        IResult res = Result.Success();
        return Task.FromResult(res);
    }
}
