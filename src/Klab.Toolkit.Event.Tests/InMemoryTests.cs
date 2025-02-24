﻿using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Klab.Toolkit.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Klab.Toolkit.Event.Tests;

public class InMemoryTests
{
    private readonly IEventBus _eventBus;
    private readonly TestEventHandler1 _testEventHandler1;
    private readonly TestEventHandler2 _testEventHandler2;

    public InMemoryTests()
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddEventSubsribtion<TestEvent1, TestEventHandler1>(ServiceLifetime.Singleton);
                services.AddEventSubsribtion<TestEvent1, TestEventHandler2>(ServiceLifetime.Singleton);
                services.UseEventModule();
            })
            .Build();

        _testEventHandler1 = host.Services.GetRequiredService<TestEventHandler1>();
        _testEventHandler2 = host.Services.GetRequiredService<TestEventHandler2>();
        _eventBus = host.Services.GetRequiredService<IEventBus>();
        host.Start();
    }

    [Fact]
    public async Task SimpleEventPublishTest()
    {
        // arrange & act
        await _eventBus.PublishAsync(new TestEvent1());
        await Task.Delay(1000); // wait for event to be processed

        // assert
        _testEventHandler1.Counter.Should().Be(1);
        _testEventHandler2.Counter.Should().Be(2);
    }

    [Fact]
    public async Task MultipleEventPublishTest()
    {
        // arrange & act
        const int count = 100_000;
        for (int i = 0; i < count; i++)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => await _eventBus.PublishAsync(new TestEvent1()));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
        await Task.Delay(2000); // wait for event to be processed

        // assert
        _testEventHandler1.Counter.Should().Be(count);
        _testEventHandler2.Counter.Should().Be(count * 2);
    }
}

internal sealed class TestEventHandler1 : IEventHandler<TestEvent1>
{
    private readonly object _lock = new();
    public int Counter { get; private set; }

    public Task<IResult> Handle(TestEvent1 notification, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            Counter++;
        }
        IResult res = Result.Success();
        return Task.FromResult(res);
    }
}

internal sealed class TestEventHandler2 : IEventHandler<TestEvent1>
{
    private readonly object _lock = new();

    public int Counter { get; private set; }

    public Task<IResult> Handle(TestEvent1 notification, CancellationToken cancellationToken)
    {
        lock (_lock)
        {
            Counter += 2;
        }
        IResult res = Result.Success();
        return Task.FromResult(res);
    }
}
