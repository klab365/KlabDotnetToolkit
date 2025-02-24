using System.Linq;
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
                services.UseEventModule();
            })
            .Build();

        _eventBus = host.Services.GetRequiredService<IEventBus>();
        host.Start();
    }

    [Fact]
    public async Task SimpleEventPublishTest()
    {
        // arrange
        _eventBus.Subscribe<TestEvent1>(IncreaseCounterAsync);

        // act
        await _eventBus.PublishAsync(new TestEvent1());
        await Task.Delay(1000); // to be processed

        // assert
        _counter.Should().Be(1);
        _eventBus.GetLocalEventHandlers().Count.Should().Be(1);
        _eventBus.GetLocalEventHandlers().First().Value.Count.Should().Be(1);
    }

    [Fact]
    public async Task SubscribeUnsubscribeTest()
    {
        // arrange
        _eventBus.Subscribe<TestEvent1>(IncreaseCounterAsync);

        // act
        await _eventBus.PublishAsync(new TestEvent1());
        await Task.Delay(1000); // to be processed

        // assert
        _counter.Should().Be(1);

        // act
        _eventBus.Unsubscribe<TestEvent1>(IncreaseCounterAsync);
        await _eventBus.PublishAsync(new TestEvent1());
        await Task.Delay(1000); // to be processed

        // assert
        _counter.Should().Be(1);
    }

    [Fact]
    public async Task Subscribe_ShouldRegisterTwoDifferentMethods()
    {
        // arrange
        _eventBus.Subscribe<TestEvent1>(IncreaseCounterAsync);
        _eventBus.Subscribe<TestEvent1>(IncreaseCounterAsync2);

        // act
        await _eventBus.PublishAsync(new TestEvent1());
        await Task.Delay(1000); // to be processed
        _eventBus.Unsubscribe<TestEvent1>(IncreaseCounterAsync);
        _eventBus.Unsubscribe<TestEvent1>(IncreaseCounterAsync2);

        // assert
        _counter.Should().Be(3);
        _eventBus.GetLocalEventHandlers().Count.Should().Be(1);
        _eventBus.GetLocalEventHandlers().First().Value.Count.Should().Be(0);
    }

    private Task<IResult> IncreaseCounterAsync(TestEvent1 @event, CancellationToken cancellationToken)
    {
        _counter++;
        IResult res = Result.Success();
        return Task.FromResult(res);
    }

    private Task<IResult> IncreaseCounterAsync2(TestEvent1 @event, CancellationToken cancellationToken)
    {
        _counter += 2;
        IResult res = Result.Success();
        return Task.FromResult(res);
    }
}
