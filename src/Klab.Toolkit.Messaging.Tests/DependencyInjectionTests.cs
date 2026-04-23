using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Klab.Toolkit.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.Messaging.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddEventHandler_WithTransientLifetime_RegistersHandlerAndWrapper()
    {
        ServiceCollection services = new();
        services.AddEventHandler<TestEvent1, TestEventHandler>(ServiceLifetime.Transient);

        ServiceProvider provider = services.BuildServiceProvider();

        provider.GetService<TestEventHandler>().Should().NotBeNull();
        provider.GetService<IEventHandler<TestEvent1>>().Should().NotBeNull();
        provider.GetService<EventHandlerWrapper<TestEvent1>>().Should().NotBeNull();
    }

    [Fact]
    public void AddEventHandler_WithSingletonLifetime_RegistersHandlerAndWrapper()
    {
        ServiceCollection services = new();
        services.AddEventHandler<TestEvent1, TestEventHandler>(ServiceLifetime.Singleton);

        ServiceProvider provider = services.BuildServiceProvider();

        TestEventHandler instance1 = provider.GetRequiredService<TestEventHandler>();
        TestEventHandler instance2 = provider.GetRequiredService<TestEventHandler>();
        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void AddEventHandler_WithScopedLifetime_RegistersHandlerAndWrapper()
    {
        ServiceCollection services = new();
        services.AddEventHandler<TestEvent1, TestEventHandler>(ServiceLifetime.Scoped);

        ServiceProvider provider = services.BuildServiceProvider();

        using IServiceScope scope1 = provider.CreateScope();
        using IServiceScope scope2 = provider.CreateScope();

        TestEventHandler instance1 = scope1.ServiceProvider.GetRequiredService<TestEventHandler>();
        TestEventHandler instance2 = scope2.ServiceProvider.GetRequiredService<TestEventHandler>();
        instance1.Should().NotBeSameAs(instance2);
    }

    [Fact]
    public void AddEventHandler_WithSingletonLifetime_InterfaceAndConcreteAreSameInstance()
    {
        ServiceCollection services = new();
        services.AddEventHandler<TestEvent1, TestEventHandler>(ServiceLifetime.Singleton);

        ServiceProvider provider = services.BuildServiceProvider();

        TestEventHandler concrete = provider.GetRequiredService<TestEventHandler>();
        IEventHandler<TestEvent1> iface = provider.GetRequiredService<IEventHandler<TestEvent1>>();
        concrete.Should().BeSameAs(iface);
    }

    [Fact]
    public void AddEventHandler_MultipleHandlersForSameEvent_RegistersBoth()
    {
        ServiceCollection services = new();
        services.AddEventHandler<TestEvent1, TestEventHandler>(ServiceLifetime.Transient);
        services.AddEventHandler<TestEvent1, TestEventHandler2>(ServiceLifetime.Transient);

        ServiceProvider provider = services.BuildServiceProvider();

        IEnumerable<IEventHandler<TestEvent1>> handlers = provider.GetServices<IEventHandler<TestEvent1>>();
        handlers.Should().HaveCount(2);
    }

    [Fact]
    public void AddRequestResponseHandler_WithTransientLifetime_RegistersHandlerAndWrapper()
    {
        ServiceCollection services = new();
        services.AddRequestResponseHandler<TestRequest, string, TestRequestHandler>(ServiceLifetime.Transient);

        ServiceProvider provider = services.BuildServiceProvider();

        provider.GetService<TestRequestHandler>().Should().NotBeNull();
        provider.GetService<IRequestHandler<TestRequest, string>>().Should().NotBeNull();
        provider.GetService<RequestResponseHandlerWrapper<TestRequest, string>>().Should().NotBeNull();
    }

    [Fact]
    public void AddRequestResponseHandler_WithSingletonLifetime_RegistersHandlerAndWrapper()
    {
        ServiceCollection services = new();
        services.AddRequestResponseHandler<TestRequest, string, TestRequestHandler>(ServiceLifetime.Singleton);

        ServiceProvider provider = services.BuildServiceProvider();

        TestRequestHandler instance1 = provider.GetRequiredService<TestRequestHandler>();
        TestRequestHandler instance2 = provider.GetRequiredService<TestRequestHandler>();
        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void AddRequestResponseHandler_WithScopedLifetime_RegistersHandlerAndWrapper()
    {
        ServiceCollection services = new();
        services.AddRequestResponseHandler<TestRequest, string, TestRequestHandler>(ServiceLifetime.Scoped);

        ServiceProvider provider = services.BuildServiceProvider();

        using IServiceScope scope1 = provider.CreateScope();
        using IServiceScope scope2 = provider.CreateScope();

        TestRequestHandler instance1 = scope1.ServiceProvider.GetRequiredService<TestRequestHandler>();
        TestRequestHandler instance2 = scope2.ServiceProvider.GetRequiredService<TestRequestHandler>();
        instance1.Should().NotBeSameAs(instance2);
    }

    [Fact]
    public void AddRequestResponseHandler_WithSingletonLifetime_InterfaceAndConcreteAreSameInstance()
    {
        ServiceCollection services = new();
        services.AddRequestResponseHandler<TestRequest, string, TestRequestHandler>(ServiceLifetime.Singleton);

        ServiceProvider provider = services.BuildServiceProvider();

        TestRequestHandler concrete = provider.GetRequiredService<TestRequestHandler>();
        IRequestHandler<TestRequest, string> iface = provider.GetRequiredService<IRequestHandler<TestRequest, string>>();
        concrete.Should().BeSameAs(iface);
    }

    [Fact]
    public void AddStreamRequestResponseHandler_WithTransientLifetime_RegistersHandlerAndWrapper()
    {
        ServiceCollection services = new();
        services.AddStreamRequestResponseHandler<TestStreamRequest, string, TestStreamRequestHandler>(ServiceLifetime.Transient);

        ServiceProvider provider = services.BuildServiceProvider();

        provider.GetService<TestStreamRequestHandler>().Should().NotBeNull();
        provider.GetService<IStreamRequestHandler<TestStreamRequest, string>>().Should().NotBeNull();
        provider.GetService<StreamRequestResponseHandlerWrapper<TestStreamRequest, string>>().Should().NotBeNull();
    }

    [Fact]
    public void AddStreamRequestResponseHandler_WithSingletonLifetime_RegistersHandlerAndWrapper()
    {
        ServiceCollection services = new();
        services.AddStreamRequestResponseHandler<TestStreamRequest, string, TestStreamRequestHandler>(ServiceLifetime.Singleton);

        ServiceProvider provider = services.BuildServiceProvider();

        TestStreamRequestHandler instance1 = provider.GetRequiredService<TestStreamRequestHandler>();
        TestStreamRequestHandler instance2 = provider.GetRequiredService<TestStreamRequestHandler>();
        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void AddStreamRequestResponseHandler_WithScopedLifetime_RegistersHandlerAndWrapper()
    {
        ServiceCollection services = new();
        services.AddStreamRequestResponseHandler<TestStreamRequest, string, TestStreamRequestHandler>(ServiceLifetime.Scoped);

        ServiceProvider provider = services.BuildServiceProvider();

        using IServiceScope scope1 = provider.CreateScope();
        using IServiceScope scope2 = provider.CreateScope();

        TestStreamRequestHandler instance1 = scope1.ServiceProvider.GetRequiredService<TestStreamRequestHandler>();
        TestStreamRequestHandler instance2 = scope2.ServiceProvider.GetRequiredService<TestStreamRequestHandler>();
        instance1.Should().NotBeSameAs(instance2);
    }

    [Fact]
    public void AddStreamRequestResponseHandler_WithSingletonLifetime_InterfaceAndConcreteAreSameInstance()
    {
        ServiceCollection services = new();
        services.AddStreamRequestResponseHandler<TestStreamRequest, string, TestStreamRequestHandler>(ServiceLifetime.Singleton);

        ServiceProvider provider = services.BuildServiceProvider();

        TestStreamRequestHandler concrete = provider.GetRequiredService<TestStreamRequestHandler>();
        IStreamRequestHandler<TestStreamRequest, string> iface = provider.GetRequiredService<IStreamRequestHandler<TestStreamRequest, string>>();
        concrete.Should().BeSameAs(iface);
    }

    private sealed record TestRequest : IRequest<string>;

    private sealed record TestStreamRequest : IStreamRequest<string>;

    private sealed class TestEventHandler : IEventHandler<TestEvent1>
    {
        public Task<Result> Handle(TestEvent1 notification, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class TestEventHandler2 : IEventHandler<TestEvent1>
    {
        public Task<Result> Handle(TestEvent1 notification, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class TestRequestHandler : IRequestHandler<TestRequest, string>
    {
        public Task<string> HandleAsync(TestRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult("response");
        }
    }

    private sealed class TestStreamRequestHandler : IStreamRequestHandler<TestStreamRequest, string>
    {
        public async IAsyncEnumerable<string> HandleAsync(TestStreamRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return await Task.FromResult("item");
        }
    }
}
