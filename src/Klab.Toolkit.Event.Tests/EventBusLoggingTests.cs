using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
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
}
