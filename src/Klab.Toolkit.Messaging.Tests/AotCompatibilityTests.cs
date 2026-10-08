using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.Messaging.Tests;

public sealed class AotCompatibilityTests
{
    [Fact]
    public void AddMessagingModule_GenericLogger_RegistersAotSafeDefaults()
    {
        ServiceCollection services = new();
        services.AddMessagingModule<TestMessagingLogger>(configuration => configuration.MessagingLoggerPath = "recording.json");

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<IEventQueue>().Should().BeOfType<InMemoryMessageQueue>();
        provider.GetRequiredService<IMessagingLogger>().Should().BeOfType<TestMessagingLogger>();
        provider.GetRequiredService<MessagingModuleConfiguration>().MessagingLoggerPath.Should().Be("recording.json");
        provider.GetRequiredService<IMediator>().Should().NotBeNull();
    }

    [Fact]
    public void AddMessagingModule_GenericHostedLogger_RegistersSameLoggerInstanceAsHostedService()
    {
        ServiceCollection services = new();
        services.AddMessagingModule<FileMessagingLogger>();
        using ServiceProvider provider = services.BuildServiceProvider();

        IMessagingLogger logger = provider.GetRequiredService<IMessagingLogger>();
        IEnumerable<Microsoft.Extensions.Hosting.IHostedService> hostedServices = provider.GetServices<Microsoft.Extensions.Hosting.IHostedService>();

        hostedServices.Should().ContainSingle(service => ReferenceEquals(service, logger));
    }

    [Fact]
    public async Task SendAsync_WithoutRegisteredHandler_ThrowsKeyNotFoundException()
    {
        ServiceCollection services = new();
        services.AddMessagingModule<NullMessagingLogger>();
        using ServiceProvider provider = services.BuildServiceProvider();

        Func<Task> act = () => provider.GetRequiredService<IMediator>().SendAsync(new UnregisteredRequest());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Stream_WithoutRegisteredHandler_ThrowsKeyNotFoundException()
    {
        ServiceCollection services = new();
        services.AddMessagingModule<NullMessagingLogger>();
        using ServiceProvider provider = services.BuildServiceProvider();
        IMediator mediator = provider.GetRequiredService<IMediator>();

        Func<Task> act = async () =>
        {
            await foreach (string _ in mediator.Stream(new UnregisteredStreamRequest()))
            {
            }
        };

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public void EventHandlerRegistration_WithWrongEventType_ThrowsInvalidOperationException()
    {
        ServiceCollection services = new();
        using ServiceProvider provider = services.BuildServiceProvider();
        EventHandlerWrapperRegistration<TestEvent1> registration = new(provider);

        Action act = () => registration.GetHandlers(provider, new TestEvent2("wrong type"));

        act.Should().Throw<InvalidOperationException>();
    }

    private sealed class TestMessagingLogger : IMessagingLogger
    {
        public ValueTask LogEventAsync(EventBase @event, Klab.Toolkit.Results.Result[] handlerResults) => ValueTask.CompletedTask;
        public ValueTask LogCommandAsync(Type requestType, object requestData, object response) => ValueTask.CompletedTask;
        public ValueTask LogStreamRequestAsync(Type requestType, object requestData, object response) => ValueTask.CompletedTask;
        public Task FlushAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed record UnregisteredRequest : IRequest<string>;

    private sealed record UnregisteredStreamRequest : IStreamRequest<string>;
}
