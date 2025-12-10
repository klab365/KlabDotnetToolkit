using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Klab.Toolkit.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Klab.Toolkit.Event.Tests;

/// <summary>
/// Comprehensive tests for EventBus to increase code coverage
/// </summary>
public class EventBusCoverageTests
{
    private readonly IEventBus _eventBus;
    private int _counter;

    public EventBusCoverageTests()
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddEventModule(cfg =>
                {
                    cfg.ShouldLogEvents = false;
                });
            })
            .Build();

        _eventBus = host.Services.GetRequiredService<IEventBus>();
        host.Start();
    }

    [Fact]
    public async Task Subscribe_ConcurrentCalls_ShouldBeThreadSafe()
    {
        // arrange
        const int taskCount = 100;
        Task<Result>[] tasks = new Task<Result>[taskCount];
        Func<TestEvent1, CancellationToken, Task<Result>>[] handlers = new Func<TestEvent1, CancellationToken, Task<Result>>[taskCount];

        // Create unique handlers
        for (int i = 0; i < taskCount; i++)
        {
            int index = i;
            handlers[i] = (evt, ct) =>
            {
                Interlocked.Add(ref _counter, index);
                return Task.FromResult(Result.Success());
            };
        }

        // act - subscribe concurrently
        for (int i = 0; i < taskCount; i++)
        {
            int index = i;
            tasks[i] = Task.Run(() => _eventBus.Subscribe<TestEvent1>(handlers[index]));
        }

        await Task.WhenAll(tasks);

        // assert - all handlers should be registered
        ConcurrentDictionary<Type, ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>>> localHandlers = _eventBus.GetLocalEventHandlers();
        localHandlers.Should().ContainKey(typeof(TestEvent1));
        localHandlers[typeof(TestEvent1)].Count.Should().Be(taskCount);
    }

    [Fact]
    public async Task Subscribe_DuplicateHandler_ShouldReturnFailure()
    {
        // arrange
        Task<Result> Handler(TestEvent1 evt, CancellationToken ct)
        {
            _counter++;
            return Task.FromResult(Result.Success());
        }

        // act
        Result result1 = _eventBus.Subscribe<TestEvent1>(Handler);
        Result result2 = _eventBus.Subscribe<TestEvent1>(Handler);

        // assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsFailure.Should().BeTrue();
        result2.Error.Message.Should().Be("Handler already exists");

        // verify only one handler is registered
        ConcurrentDictionary<Type, ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>>> handlers = _eventBus.GetLocalEventHandlers();
        handlers[typeof(TestEvent1)].Count.Should().Be(1);

        // cleanup
        await Task.CompletedTask;
    }

    [Fact]
    public void Unsubscribe_NonExistentEventType_ShouldReturnFailure()
    {
        // arrange - no subscription made
        Task<Result> Handler(TestEvent1 evt, CancellationToken ct) => Task.FromResult(Result.Success());

        // act
        Result result = _eventBus.Unsubscribe<TestEvent1>(Handler);

        // assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("No handler found for the event type");
    }

    [Fact]
    public void Unsubscribe_NonExistentHandler_ShouldSucceedButNotRemoveAnything()
    {
        // arrange
        Task<Result> Handler1(TestEvent1 evt, CancellationToken ct)
        {
            _counter++;
            return Task.FromResult(Result.Success());
        }

        Task<Result> Handler2(TestEvent1 evt, CancellationToken ct)
        {
            _counter += 2;
            return Task.FromResult(Result.Success());
        }

        _eventBus.Subscribe<TestEvent1>(Handler1);
        int initialCount = _eventBus.GetLocalEventHandlers()[typeof(TestEvent1)].Count;

        // act - try to unsubscribe a handler that was never subscribed
        Result result = _eventBus.Unsubscribe<TestEvent1>(Handler2);

        // assert
        result.IsSuccess.Should().BeTrue();
        int currentCount = _eventBus.GetLocalEventHandlers()[typeof(TestEvent1)].Count;
        currentCount.Should().Be(initialCount); // Handler1 should still be there
    }

    [Fact]
    public async Task Unsubscribe_ConcurrentCalls_ShouldBeThreadSafe()
    {
        // arrange
        const int handlerCount = 50;
        Func<TestEvent1, CancellationToken, Task<Result>>[] handlers = new Func<TestEvent1, CancellationToken, Task<Result>>[handlerCount];

        // Create and subscribe unique handlers
        for (int i = 0; i < handlerCount; i++)
        {
            int index = i;
            handlers[i] = (evt, ct) =>
            {
                Interlocked.Add(ref _counter, index);
                return Task.FromResult(Result.Success());
            };
            _eventBus.Subscribe<TestEvent1>(handlers[i]);
        }

        int initialCount = _eventBus.GetLocalEventHandlers()[typeof(TestEvent1)].Count;
        initialCount.Should().Be(handlerCount);

        // act - unsubscribe concurrently
        Task[] tasks = new Task[handlerCount];
        for (int i = 0; i < handlerCount; i++)
        {
            int index = i;
            tasks[i] = Task.Run(() => _eventBus.Unsubscribe<TestEvent1>(handlers[index]));
        }

        await Task.WhenAll(tasks);

        // assert - all handlers should be removed with retry logic
        int finalCount = _eventBus.GetLocalEventHandlers()[typeof(TestEvent1)].Count;
        finalCount.Should().Be(0);
    }

    [Fact]
    public async Task PublishAsync_WithCancellationToken_ShouldCancelGracefully()
    {
        // arrange
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.Cancel();

        // act
        Result result = await _eventBus.PublishAsync(new TestEvent1(), cts.Token);

        // assert - even though canceled, it should return success
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task PublishAsync_MultipleEventsRapidly_ShouldHandleAllEvents()
    {
        // arrange
        const int eventCount = 1000;
        _eventBus.Subscribe<TestEvent1>((evt, ct) =>
        {
            Interlocked.Increment(ref _counter);
            return Task.FromResult(Result.Success());
        });

        // act
        Task<Result>[] tasks = new Task<Result>[eventCount];
        for (int i = 0; i < eventCount; i++)
        {
            tasks[i] = _eventBus.PublishAsync(new TestEvent1());
        }

        await Task.WhenAll(tasks);
        await Task.Delay(2000); // wait for processing

        // assert
        _counter.Should().Be(eventCount);
        tasks.All(t => t.Result.IsSuccess).Should().BeTrue();
    }

    [Fact]
    public async Task Subscribe_MultipleEventTypes_ShouldMaintainSeparateHandlers()
    {
        // arrange
        int counter1 = 0;
        int counter2 = 0;

        _eventBus.Subscribe<TestEvent1>((evt, ct) =>
        {
            Interlocked.Increment(ref counter1);
            return Task.FromResult(Result.Success());
        });

        _eventBus.Subscribe<TestEvent2>((evt, ct) =>
        {
            Interlocked.Increment(ref counter2);
            return Task.FromResult(Result.Success());
        });

        // act
        await _eventBus.PublishAsync(new TestEvent1());
        await _eventBus.PublishAsync(new TestEvent2("test"));
        await Task.Delay(1000);

        // assert
        counter1.Should().Be(1);
        counter2.Should().Be(1);

        ConcurrentDictionary<Type, ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>>> handlers = _eventBus.GetLocalEventHandlers();
        handlers.Should().ContainKey(typeof(TestEvent1));
        handlers.Should().ContainKey(typeof(TestEvent2));
    }

    [Fact]
    public async Task GetLocalEventHandlers_ShouldReturnCurrentState()
    {
        // arrange
        Func<TestEvent1, CancellationToken, Task<Result>> handler1 = (evt, ct) => Task.FromResult(Result.Success());
        Func<TestEvent1, CancellationToken, Task<Result>> handler2 = (evt, ct) => Task.FromResult(Result.Success());

        // act & assert - initially empty or doesn't contain TestEvent1
        ConcurrentDictionary<Type, ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>>> initialHandlers = _eventBus.GetLocalEventHandlers();
        initialHandlers.ContainsKey(typeof(TestEvent1)).Should().BeFalse();

        // subscribe first handler
        _eventBus.Subscribe<TestEvent1>(handler1);
        ConcurrentDictionary<Type, ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>>> afterFirst = _eventBus.GetLocalEventHandlers();
        afterFirst[typeof(TestEvent1)].Count.Should().Be(1);

        // subscribe second handler
        _eventBus.Subscribe<TestEvent1>(handler2);
        ConcurrentDictionary<Type, ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>>> afterSecond = _eventBus.GetLocalEventHandlers();
        afterSecond[typeof(TestEvent1)].Count.Should().Be(2);

        // unsubscribe first handler
        _eventBus.Unsubscribe<TestEvent1>(handler1);
        ConcurrentDictionary<Type, ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>>> afterUnsubscribe = _eventBus.GetLocalEventHandlers();
        afterUnsubscribe[typeof(TestEvent1)].Count.Should().Be(1);

        await Task.CompletedTask;
    }

    [Fact]
    public async Task Subscribe_AndPublish_WithCancellationToken_ShouldPassTokenToHandler()
    {
        // arrange
        bool tokenReceived = false;
        CancellationTokenSource cts = new CancellationTokenSource();

        _eventBus.Subscribe<TestEvent1>((evt, ct) =>
        {
            tokenReceived = ct.CanBeCanceled;
            return Task.FromResult(Result.Success());
        });

        // act
        await _eventBus.PublishAsync(new TestEvent1(), cts.Token);
        await Task.Delay(500);

        // assert
        tokenReceived.Should().BeTrue();
    }

    [Fact]
    public async Task MessageQueue_ShouldBeAccessible()
    {
        // arrange & act
        IEventQueue queue = _eventBus.MessageQueue;

        // assert
        queue.Should().NotBeNull();
        queue.Should().BeAssignableTo<IEventQueue>();

        await Task.CompletedTask;
    }

    [Fact]
    public async Task Subscribe_Unsubscribe_Subscribe_ShouldWorkCorrectly()
    {
        // arrange
        Task<Result> Handler(TestEvent1 evt, CancellationToken ct)
        {
            Interlocked.Increment(ref _counter);
            return Task.FromResult(Result.Success());
        }

        // act - subscribe
        Result result1 = _eventBus.Subscribe<TestEvent1>(Handler);
        await _eventBus.PublishAsync(new TestEvent1());
        await Task.Delay(500);

        // unsubscribe
        Result result2 = _eventBus.Unsubscribe<TestEvent1>(Handler);
        await _eventBus.PublishAsync(new TestEvent1());
        await Task.Delay(500);

        // subscribe again
        Result result3 = _eventBus.Subscribe<TestEvent1>(Handler);
        await _eventBus.PublishAsync(new TestEvent1());
        await Task.Delay(500);

        // assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result3.IsSuccess.Should().BeTrue();
        _counter.Should().Be(2); // Only first and third publish should increment
    }

    [Fact]
    public async Task ConcurrentSubscribeAndUnsubscribe_ShouldMaintainConsistency()
    {
        // arrange
        const int iterations = 100;
        int handler1Called = 0;
        int handler2Called = 0;

        Task<Result> Handler1(TestEvent1 evt, CancellationToken ct)
        {
            Interlocked.Increment(ref handler1Called);
            return Task.FromResult(Result.Success());
        }

        Task<Result> Handler2(TestEvent1 evt, CancellationToken ct)
        {
            Interlocked.Increment(ref handler2Called);
            return Task.FromResult(Result.Success());
        }

        // act - concurrent subscribe/unsubscribe operations
        Task[] tasks = new Task[iterations * 4];
        for (int i = 0; i < iterations; i++)
        {
            tasks[i * 4] = Task.Run(() => _eventBus.Subscribe<TestEvent1>(Handler1));
            tasks[i * 4 + 1] = Task.Run(() => _eventBus.Subscribe<TestEvent1>(Handler2));
            tasks[i * 4 + 2] = Task.Run(() => _eventBus.Unsubscribe<TestEvent1>(Handler1));
            tasks[i * 4 + 3] = Task.Run(() => _eventBus.Unsubscribe<TestEvent1>(Handler2));
        }

        await Task.WhenAll(tasks);

        // assert - no exceptions should be thrown and state should be consistent
        ConcurrentDictionary<Type, ConcurrentBag<KeyValuePair<int, Func<EventBase, CancellationToken, Task<Result>>>>> handlers = _eventBus.GetLocalEventHandlers();
        if (handlers.ContainsKey(typeof(TestEvent1)))
        {
            // Count should be reasonable (0-2 depending on final state)
            handlers[typeof(TestEvent1)].Count.Should().BeLessOrEqualTo(2);
        }
    }
}
