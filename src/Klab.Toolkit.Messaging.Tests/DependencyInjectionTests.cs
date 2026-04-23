using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Klab.Toolkit.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

public class RegisterEventQueueTests
{
    [Fact]
    public void AddMessagingModule_WithDefaultConfiguration_RegistersInMemoryMessageQueueAsSingleton()
    {
        ServiceCollection services = new();
        services.AddMessagingModule();

        ServiceProvider provider = services.BuildServiceProvider();

        IEventQueue instance1 = provider.GetRequiredService<IEventQueue>();
        IEventQueue instance2 = provider.GetRequiredService<IEventQueue>();
        instance1.Should().BeOfType<InMemoryMessageQueue>();
        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void AddMessagingModule_WithCustomEventQueueType_RegistersCustomType()
    {
        ServiceCollection services = new();
        services.AddMessagingModule(cfg => cfg.EventQueueType = typeof(CustomEventQueue));

        ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<IEventQueue>().Should().BeOfType<CustomEventQueue>();
    }

    [Fact]
    public void AddMessagingModule_WithTransientEventQueueLifetime_RegistersTransient()
    {
        ServiceCollection services = new();
        services.AddMessagingModule(cfg => cfg.EventQueueLifetime = ServiceLifetime.Transient);

        ServiceProvider provider = services.BuildServiceProvider();

        IEventQueue instance1 = provider.GetRequiredService<IEventQueue>();
        IEventQueue instance2 = provider.GetRequiredService<IEventQueue>();
        instance1.Should().NotBeSameAs(instance2);
    }

    [Fact]
    public void AddMessagingModule_WithNullEventQueueType_ThrowsInvalidOperationException()
    {
        ServiceCollection services = new();
        Action act = () => services.AddMessagingModule(cfg => cfg.EventQueueType = null);

        act.Should().Throw<InvalidOperationException>().WithMessage("*Event queue type is not set*");
    }

    [Fact]
    public void AddMessagingModule_WithInvalidEventQueueType_ThrowsArgumentException()
    {
        ServiceCollection services = new();
        Action act = () => services.AddMessagingModule(cfg => cfg.EventQueueType = typeof(NotAnEventQueue));

        act.Should().Throw<ArgumentException>().WithMessage("*Invalid event queue type*");
    }

    private sealed class CustomEventQueue : IEventQueue
    {
        public Task EnqueueAsync(EventBase @event, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public async IAsyncEnumerable<EventBase> DequeueAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return await Task.FromResult<EventBase>(new TestEvent1());
        }
    }

    private sealed class NotAnEventQueue { }
}

public class RegisterMessagingLoggerTests
{
    [Fact]
    public void AddMessagingModule_WithDefaultConfiguration_RegistersNullMessagingLoggerAsSingleton()
    {
        ServiceCollection services = new();
        services.AddMessagingModule();

        ServiceProvider provider = services.BuildServiceProvider();

        IMessagingLogger instance1 = provider.GetRequiredService<IMessagingLogger>();
        IMessagingLogger instance2 = provider.GetRequiredService<IMessagingLogger>();
        instance1.Should().BeOfType<NullMessagingLogger>();
        instance1.Should().BeSameAs(instance2);
    }

    [Fact]
    public void AddMessagingModule_WithNullMessagingLoggerType_ThrowsInvalidOperationException()
    {
        ServiceCollection services = new();
        Action act = () => services.AddMessagingModule(cfg => cfg.MessagingLoggerType = null);

        act.Should().Throw<InvalidOperationException>().WithMessage("*Messaging logger type is not set*");
    }

    [Fact]
    public void AddMessagingModule_WithInvalidMessagingLoggerType_ThrowsArgumentException()
    {
        ServiceCollection services = new();
        Action act = () => services.AddMessagingModule(cfg => cfg.MessagingLoggerType = typeof(NotAMessagingLogger));

        act.Should().Throw<ArgumentException>().WithMessage("*Invalid messaging logger type*");
    }

    [Fact]
    public void AddMessagingModule_WithHostedServiceLogger_RegistersLoggerAsSingleton()
    {
        ServiceCollection services = new();

        Action act = () => services.AddMessagingModule(cfg => cfg.MessagingLoggerType = typeof(HostedMessagingLogger));

        act.Should().Throw<ArgumentException>();
    }

    private sealed class NotAMessagingLogger { }

    private sealed class HostedMessagingLogger : IMessagingLogger, IHostedService
    {
        public ValueTask LogEventAsync(EventBase @event, Result[] handlerResults) => ValueTask.CompletedTask;
        public ValueTask LogCommandAsync(Type requestType, object requestData, object response) => ValueTask.CompletedTask;
        public ValueTask LogStreamRequestAsync(Type requestType, object requestData, object response) => ValueTask.CompletedTask;
        public Task FlushAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

public class AddGlobalRequestMiddlewareTests
{
    [Fact]
    public void AddGlobalRequestMiddleware_WithValidType_RegistersOpenGenericDescriptor()
    {
        ServiceCollection services = new();
        services.AddGlobalRequestMiddleware(typeof(GlobalMiddleware<,>));

        ServiceDescriptor descriptor = services.Should().ContainSingle(d => d.ServiceType == typeof(IRequestMiddleware<,>)).Subject;
        descriptor.ImplementationType.Should().Be(typeof(GlobalMiddleware<,>));
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddGlobalRequestMiddleware_WithTransientLifetime_RegistersTransient()
    {
        ServiceCollection services = new();
        services.AddGlobalRequestMiddleware(typeof(GlobalMiddleware<,>), ServiceLifetime.Transient);

        ServiceDescriptor descriptor = services.Should().ContainSingle(d => d.ServiceType == typeof(IRequestMiddleware<,>)).Subject;
        descriptor.Lifetime.Should().Be(ServiceLifetime.Transient);
    }

    [Fact]
    public void AddGlobalRequestMiddleware_WithNullServices_ThrowsArgumentNullException()
    {
        IServiceCollection services = null!;
        Action act = () => services.AddGlobalRequestMiddleware(typeof(GlobalMiddleware<,>));

        act.Should().Throw<ArgumentNullException>().WithParameterName("services");
    }

    [Fact]
    public void AddGlobalRequestMiddleware_WithNullMiddlewareType_ThrowsArgumentNullException()
    {
        ServiceCollection services = new();
        Action act = () => services.AddGlobalRequestMiddleware(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("middlewareType");
    }

    [Fact]
    public void AddGlobalRequestMiddleware_WithClosedGenericType_ThrowsArgumentException()
    {
        ServiceCollection services = new();
        Action act = () => services.AddGlobalRequestMiddleware(typeof(List<string>));

        act.Should().Throw<ArgumentException>().WithMessage("*open generic*");
    }

    [Fact]
    public void AddGlobalRequestMiddleware_WithSingleTypeParameter_ThrowsArgumentException()
    {
        ServiceCollection services = new();
        Action act = () => services.AddGlobalRequestMiddleware(typeof(SingleParamMiddleware<>));

        act.Should().Throw<ArgumentException>().WithMessage("*two generic parameters*");
    }

    [Fact]
    public void AddGlobalRequestMiddleware_WithAbstractType_ThrowsArgumentException()
    {
        ServiceCollection services = new();
        Action act = () => services.AddGlobalRequestMiddleware(typeof(AbstractMiddleware<,>));

        act.Should().Throw<ArgumentException>().WithMessage("*concrete class*");
    }

    [Fact]
    public void AddGlobalRequestMiddleware_WithTypeNotImplementingInterface_ThrowsArgumentException()
    {
        ServiceCollection services = new();
        Action act = () => services.AddGlobalRequestMiddleware(typeof(NotAMiddleware<,>));

        act.Should().Throw<ArgumentException>().WithMessage("*IRequestMiddleware*");
    }

    [Fact]
    public void AddGlobalRequestMiddleware_ReturnsServiceCollection()
    {
        ServiceCollection services = new();
        IServiceCollection result = services.AddGlobalRequestMiddleware(typeof(GlobalMiddleware<,>));

        result.Should().BeSameAs(services);
    }

    private sealed class GlobalMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : notnull
    {
        public Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            => next();
    }

    private sealed class SingleParamMiddleware<T> { }

    private abstract class AbstractMiddleware<TRequest, TResponse> : IRequestMiddleware<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : notnull
    {
        public abstract Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
    }

    private sealed class NotAMiddleware<TRequest, TResponse> { }
}
