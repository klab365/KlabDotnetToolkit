using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Klab.Toolkit.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Klab.Toolkit.Event.Tests;

public class EventBusLoggingTests
{
    private readonly string _logPath;
    private readonly IEventBus _eventBus;

    public EventBusLoggingTests()
    {
        _logPath = Path.Combine(Path.GetDirectoryName(typeof(EventBusLoggingTests).Assembly.Location) ?? ".", "event-log-coverage.json");

        IHost host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddRequestResponseHandler<LoggingTestRequest, Result, LoggingTestRequestHandler>();
                services.AddRequestResponseHandler<LoggingTestRequest2, Result<string>, LoggingTestRequest2Handler>();
                services.AddEventModule(cfg =>
                {
                    cfg.ShouldLogEvents = true;
                    cfg.EventLogFilePath = _logPath;
                });
            })
            .Build();

        _eventBus = host.Services.GetRequiredService<IEventBus>();
        host.Start();
    }

    [Fact]
    public async Task PublishAsync_WithLoggingEnabled_ShouldLogEvent()
    {
        if (File.Exists(_logPath))
        {
            File.Delete(_logPath);
        }

        await _eventBus.PublishAsync(new TestEvent1());
        await Task.Delay(100);

        File.Exists(_logPath).Should().BeTrue();
        string content = File.ReadAllText(_logPath);
        content.Should().Contain("TestEvent1");
    }

    [Fact]
    public async Task PublishAsync_MultipleEvents_ShouldAccumulateLogs()
    {
        if (File.Exists(_logPath))
        {
            File.Delete(_logPath);
        }

        await _eventBus.PublishAsync(new TestEvent1());
        await Task.Delay(50);
        await _eventBus.PublishAsync(new TestEvent2("test"));
        await Task.Delay(50);

        string content = File.ReadAllText(_logPath);
        content.Should().Contain("TestEvent1");
        content.Should().Contain("TestEvent2");
    }

    [Fact]
    public async Task PublishAsync_DifferentEventTypes_ShouldLogAll()
    {
        if (File.Exists(_logPath))
        {
            File.Delete(_logPath);
        }

        await _eventBus.PublishAsync(new TestEvent1());
        await _eventBus.PublishAsync(new TestEvent2("hello"));
        await _eventBus.PublishAsync(new TestEvent1());
        await Task.Delay(100);

        string content = File.ReadAllText(_logPath);
        content.Should().Contain("TestEvent1");
        content.Should().Contain("TestEvent2");
    }

    [Fact]
    public async Task SendAsync_WithLoggingEnabled_ShouldLogRequest()
    {
        if (File.Exists(_logPath))
        {
            File.Delete(_logPath);
        }

        await _eventBus.SendAsync(new LoggingTestRequest(), CancellationToken.None);
        await Task.Delay(100);

        File.Exists(_logPath).Should().BeTrue();
        string content = File.ReadAllText(_logPath);
        content.Should().Contain("\"Request\"");
        content.Should().Contain("\"Stage\":\"Sent\"");
    }

    [Fact]
    public async Task SendAsync_MultipleRequests_ShouldAccumulateLogs()
    {
        if (File.Exists(_logPath))
        {
            File.Delete(_logPath);
        }

        await _eventBus.SendAsync(new LoggingTestRequest(), CancellationToken.None);
        await Task.Delay(50);
        await _eventBus.SendAsync(new LoggingTestRequest2("Hi"), CancellationToken.None);
        await Task.Delay(50);

        string content = File.ReadAllText(_logPath);
        content.Should().Contain("\"Request\"");
        content.Should().Contain("\"Stage\":\"Sent\"");
    }
}

public sealed record LoggingTestRequest : IRequest<Result>;

internal sealed class LoggingTestRequestHandler : IRequestHandler<LoggingTestRequest, Result>
{
    public Task<Result> HandleAsync(LoggingTestRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success());
    }
}

public sealed record LoggingTestRequest2(string Message) : IRequest<Result<string>>;

internal sealed class LoggingTestRequest2Handler : IRequestHandler<LoggingTestRequest2, Result<string>>
{
    public Task<Result<string>> HandleAsync(LoggingTestRequest2 request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result.Success(request.Message + " Pong"));
    }
}
