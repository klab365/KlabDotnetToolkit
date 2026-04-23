using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Klab.Toolkit.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Klab.Toolkit.Messaging.Tests;

public class DelegateEqualityTests
{
    private readonly IMediator _mediator;

    public DelegateEqualityTests()
    {
        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services => services.AddMessagingModule())
            .Build();

        _mediator = host.Services.GetRequiredService<IMediator>();
        host.Start();
    }

    [Fact]
    public void Subscribe_SameInstanceMethod_TwiceIsRejected()
    {
        Result first = _mediator.Subscribe<TestEvent1>(InstanceHandler);
        Result second = _mediator.Subscribe<TestEvent1>(InstanceHandler);

        first.IsSuccess.Should().BeTrue();
        second.IsFailure.Should().BeTrue();
        _mediator.GetLocalEventHandlers()[typeof(TestEvent1)].Count.Should().Be(1);
    }

    [Fact]
    public void Subscribe_SameStaticMethod_TwiceIsRejected()
    {
        Result first = _mediator.Subscribe<TestEvent1>(StaticHandler);
        Result second = _mediator.Subscribe<TestEvent1>(StaticHandler);

        first.IsSuccess.Should().BeTrue();
        second.IsFailure.Should().BeTrue();
        _mediator.GetLocalEventHandlers()[typeof(TestEvent1)].Count.Should().Be(1);
    }

    [Fact]
    public void Subscribe_DifferentInstanceMethods_BothAccepted()
    {
        Result first = _mediator.Subscribe<TestEvent1>(InstanceHandler);
        Result second = _mediator.Subscribe<TestEvent1>(InstanceHandler2);

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        _mediator.GetLocalEventHandlers()[typeof(TestEvent1)].Count.Should().Be(2);
    }

    [Fact]
    public void Unsubscribe_StaticMethod_RemovesCorrectHandler()
    {
        _mediator.Subscribe<TestEvent1>(StaticHandler);
        _mediator.Subscribe<TestEvent1>(InstanceHandler);

        Result result = _mediator.Unsubscribe<TestEvent1>(StaticHandler);

        result.IsSuccess.Should().BeTrue();
        _mediator.GetLocalEventHandlers()[typeof(TestEvent1)].Count.Should().Be(1);
    }

    [Fact]
    public async Task Subscribe_StaticMethod_IsInvokedOnPublish()
    {
        int counter = 0;
        _mediator.Subscribe<TestEvent1>((_, _) =>
        {
            counter++;
            return Task.FromResult(Result.Success());
        });

        await _mediator.PublishAsync(new TestEvent1());
        await Task.Delay(500);

        counter.Should().Be(1);
    }

    [Fact]
    public void Subscribe_SameLambdaVariable_TwiceIsRejected()
    {
        Func<TestEvent1, CancellationToken, Task<Result>> lambda =
            (_, _) => Task.FromResult(Result.Success());

        Result first = _mediator.Subscribe<TestEvent1>(lambda);
        Result second = _mediator.Subscribe<TestEvent1>(lambda);

        first.IsSuccess.Should().BeTrue();
        second.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Subscribe_TwoDistinctLambdas_BothAccepted()
    {
        Func<TestEvent1, CancellationToken, Task<Result>> lambda1 =
            (_, _) => Task.FromResult(Result.Success());
        Func<TestEvent1, CancellationToken, Task<Result>> lambda2 =
            (_, _) => Task.FromResult(Result.Success());

        Result first = _mediator.Subscribe<TestEvent1>(lambda1);
        Result second = _mediator.Subscribe<TestEvent1>(lambda2);

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        _mediator.GetLocalEventHandlers()[typeof(TestEvent1)].Count.Should().Be(2);
    }

    private static Task<Result> StaticHandler(TestEvent1 _, CancellationToken __)
    {
        return Task.FromResult(Result.Success());
    }

    private Task<Result> InstanceHandler(TestEvent1 _, CancellationToken __)
    {
        return Task.FromResult(Result.Success());
    }

    private Task<Result> InstanceHandler2(TestEvent1 _, CancellationToken __)
    {
        return Task.FromResult(Result.Success());
    }
}
