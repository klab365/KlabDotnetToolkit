using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using FluentAssertions;
using Klab.Toolkit.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Klab.Toolkit.Messaging.Tests;

public sealed class ReliabilityTests
{
    [Fact]
    public async Task ProcessingFailure_ShouldRetryAndDeadLetterAfterExhaustion()
    {
        using IHost host = CreateHost(configuration =>
        {
            configuration.EventProcessingRetryCount = 2;
            configuration.EventProcessingRetryDelay = TimeSpan.FromMilliseconds(1);
        });
        RetryHandler handler = host.Services.GetRequiredService<RetryHandler>();
        TestDeadLetterQueue queue = host.Services.GetRequiredService<TestDeadLetterQueue>();
        IMediator mediator = host.Services.GetRequiredService<IMediator>();
        await host.StartAsync();

        await mediator.PublishAsync(new RetryEvent());
        await WaitUntilAsync(() => queue.Failures.Count == 1);

        handler.AttemptCount.Should().Be(3);
        queue.Failures.Should().ContainSingle();
        queue.Failures.Single().AttemptCount.Should().Be(3);

        await host.StopAsync();
    }

    [Fact]
    public async Task ProcessingTimeout_ShouldDeadLetterEvent()
    {
        using IHost host = CreateHost(configuration => configuration.EventProcessingTimeout = TimeSpan.FromMilliseconds(25));
        TestDeadLetterQueue queue = host.Services.GetRequiredService<TestDeadLetterQueue>();
        IMediator mediator = host.Services.GetRequiredService<IMediator>();
        await host.StartAsync();

        await mediator.PublishAsync(new RetryEvent { Key = "timeout" });
        await WaitUntilAsync(() => queue.Failures.Count == 1);

        queue.Failures.Single().Exception.Should().BeOfType<TimeoutException>();

        await host.StopAsync();
    }

    [Fact]
    public async Task DuplicateIdempotencyKey_ShouldProcessEachDeliveryWhenDisabled()
    {
        using IHost host = CreateHost(configuration => configuration.EnableIdempotency = false);
        RetryHandler handler = host.Services.GetRequiredService<RetryHandler>();
        IMediator mediator = host.Services.GetRequiredService<IMediator>();
        await host.StartAsync();

        await mediator.PublishAsync(new RetryEvent { Key = "same" });
        await mediator.PublishAsync(new RetryEvent { Key = "same" });
        await WaitUntilAsync(() => handler.AttemptCount == 2);

        handler.AttemptCount.Should().Be(2);

        await host.StopAsync();
    }

    [Fact]
    public void InvalidReliabilityConfiguration_ShouldThrow()
    {
        Action negativeRetryCount = () => AddMessagingModule(configuration => configuration.EventProcessingRetryCount = -1);
        Action negativeRetryDelay = () => AddMessagingModule(configuration => configuration.EventProcessingRetryDelay = TimeSpan.FromMilliseconds(-1));
        Action invalidBackoffFactor = () => AddMessagingModule(configuration => configuration.EventProcessingRetryBackoffFactor = 0.5);
        Action invalidTimeout = () => AddMessagingModule(configuration => configuration.EventProcessingTimeout = TimeSpan.Zero);

        negativeRetryCount.Should().Throw<ArgumentOutOfRangeException>();
        negativeRetryDelay.Should().Throw<ArgumentOutOfRangeException>();
        invalidBackoffFactor.Should().Throw<ArgumentOutOfRangeException>();
        invalidTimeout.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task DuplicateIdempotencyKey_ShouldOnlyProcessOnce()
    {
        using IHost host = CreateHost();
        RetryHandler handler = host.Services.GetRequiredService<RetryHandler>();
        IMediator mediator = host.Services.GetRequiredService<IMediator>();
        await host.StartAsync();

        await mediator.PublishAsync(new RetryEvent { Key = "same" });
        await WaitUntilAsync(() => handler.AttemptCount == 1);
        await mediator.PublishAsync(new RetryEvent { Key = "same" });
        await Task.Delay(100);

        handler.AttemptCount.Should().Be(1);

        await host.StopAsync();
    }

    private static IHost CreateHost(Action<MessagingModuleConfiguration> configure = null)
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<TestDeadLetterQueue>();
                services.AddEventHandler<RetryEvent, RetryHandler>(ServiceLifetime.Singleton);
                services.AddMessagingModule(configuration =>
                {
                    configuration.MessagingLoggerType = typeof(NullMessagingLogger);
                    configure?.Invoke(configuration);
                });
                services.AddSingleton<IEventQueue>(provider => provider.GetRequiredService<TestDeadLetterQueue>());
            })
            .Build();
    }

    private static void AddMessagingModule(Action<MessagingModuleConfiguration> configure)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddMessagingModule(configure);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(5));
        while (!condition())
        {
            await Task.Delay(10, cancellationTokenSource.Token);
        }
    }
}

internal sealed record RetryEvent : EventBase
{
    public string Key { get; init; } = Guid.NewGuid().ToString("N");
    public override string IdempotencyKey => Key;
}

internal sealed class RetryHandler : IEventHandler<RetryEvent>
{
    public int AttemptCount { get; private set; }

    public async Task<Result> Handle(RetryEvent notification, CancellationToken cancellationToken)
    {
        AttemptCount++;
        if (notification.Key == "timeout")
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }

        return notification.Key == "same"
            ? Result.Success()
            : Result.Failure(Error.Create("FAILED", "Failed"));
    }
}

public sealed class TestDeadLetterQueue : IEventQueue, IDeadLetterEventQueue
{
    private readonly Channel<EventBase> _events = Channel.CreateUnbounded<EventBase>();

    public ConcurrentQueue<EventProcessingFailure> Failures { get; } = new();

    public IAsyncEnumerable<EventBase> DequeueAsync(CancellationToken cancellationToken = default)
    {
        return _events.Reader.ReadAllAsync(cancellationToken);
    }

    public async Task EnqueueAsync(EventBase @event, CancellationToken cancellationToken = default)
    {
        await _events.Writer.WriteAsync(@event, cancellationToken);
    }

    public Task DeadLetterAsync(EventBase @event, EventProcessingFailure failure, CancellationToken cancellationToken = default)
    {
        Failures.Enqueue(failure);
        return Task.CompletedTask;
    }
}
