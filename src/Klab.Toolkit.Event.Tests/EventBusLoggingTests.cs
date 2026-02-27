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
        // arrange
        if (File.Exists(_logPath))
        {
            File.Delete(_logPath);
        }

        // act
        await _eventBus.PublishAsync(new TestEvent1());
        await Task.Delay(100);

        // assert - file should exist and contain logged event
        File.Exists(_logPath).Should().BeTrue();
        string content = File.ReadAllText(_logPath);
        content.Should().Contain("TestEvent1");
    }
}
