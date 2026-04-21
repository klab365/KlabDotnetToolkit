using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Klab.Toolkit.Results;

namespace Klab.Toolkit.Messaging.Tests;

public class EventBusLoggerTests : IAsyncDisposable
{
    private readonly string _testLogPath;
    private readonly FileMessagingLogger _logger;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _backgroundTask;

    public EventBusLoggerTests()
    {
        _testLogPath = Path.Combine(Path.GetTempPath(), $"test-events-{Guid.NewGuid()}.json");
        _logger = new FileMessagingLogger(new MessagingModuleConfiguration { MessagingLoggerPath = _testLogPath });
        _backgroundTask = _logger.StartAsync(_cts.Token);
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        await _logger.StopAsync(CancellationToken.None);
        if (File.Exists(_testLogPath))
        {
            File.Delete(_testLogPath);
        }
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task FileEventBusLogger_Constructor_ShouldInitializeWithCorrectSettings()
    {
        _logger.Should().NotBeNull();
    }

    [Fact]
    public async Task FileEventBusLogger_LogEventAsync_WhenLoggingEnabled_ShouldCreateLogFile()
    {
        TestEvent1 testEvent = new TestEvent1();
        Result[] results = [Result.Success()];

        await _logger.LogEventAsync(testEvent, results);
        await _logger.FlushAsync();

        File.Exists(_testLogPath).Should().BeTrue();
        string logContent = File.ReadAllText(_testLogPath);
        logContent.Should().Contain("Event");
        logContent.Should().Contain("TestEvent1");
    }

    [Fact]
    public async Task FileEventBusLogger_LogCommandAsync_ShouldCreateLogFile()
    {
        TestRequest request = new TestRequest { Value = "test" };
        TestResponse response = new TestResponse { Result = "success" };

        await _logger.LogCommandAsync(typeof(TestRequest), request, response);
        await _logger.FlushAsync();

        File.Exists(_testLogPath).Should().BeTrue();
        string logContent = File.ReadAllText(_testLogPath);
        logContent.Should().Contain("Command");
        logContent.Should().Contain("TestRequest");
    }

    [Fact]
    public async Task FileEventBusLogger_LogEventAsync_WithFailedResults_ShouldLogErrors()
    {
        TestEvent1 testEvent = new TestEvent1();
        Result[] results = [Result.Failure(Error.Create("TestError", "Test error message"))];

        await _logger.LogEventAsync(testEvent, results);
        await _logger.FlushAsync();

        File.Exists(_testLogPath).Should().BeTrue();
        string logContent = File.ReadAllText(_testLogPath);
        logContent.Should().Contain("Test error message");
    }

    [Fact]
    public async Task FileEventBusLogger_MultipleLogs_ShouldAppendToSameFile()
    {
        TestEvent1 testEvent = new TestEvent1();
        TestRequest request = new TestRequest { Value = "test" };

        await _logger.LogEventAsync(testEvent, [Result.Success()]);
        await _logger.LogCommandAsync(typeof(TestRequest), request, null);
        await _logger.FlushAsync();

        File.Exists(_testLogPath).Should().BeTrue();
        string logContent = File.ReadAllText(_testLogPath);
        logContent.Should().Contain("Event");
        logContent.Should().Contain("Command");
    }

    [Fact]
    public async Task NullEventBusLogger_LogEventAsync_ShouldNotThrow()
    {
        NullMessagingLogger logger = new NullMessagingLogger();
        TestEvent1 testEvent = new TestEvent1();
        Result[] results = [Result.Success()];

        Func<Task> act = async () => await logger.LogEventAsync(testEvent, results);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NullEventBusLogger_LogCommandAsync_ShouldNotThrow()
    {
        NullMessagingLogger logger = new NullMessagingLogger();
        TestRequest request = new TestRequest { Value = "test" };

        Func<Task> act = async () => await logger.LogCommandAsync(typeof(TestRequest), request, null);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task FileEventBusLogger_LogEventAsync_ShouldIncludeTimestamp()
    {
        TestEvent1 testEvent = new TestEvent1();
        Result[] results = [Result.Success()];

        await _logger.LogEventAsync(testEvent, results);
        await _logger.FlushAsync();

        File.Exists(_testLogPath).Should().BeTrue();
        string logContent = File.ReadAllText(_testLogPath);
        logContent.Should().Contain("Timestamp");
    }

    [Fact]
    public async Task FileEventBusLogger_LogCommandAsync_ShouldIncludeTimestamp()
    {
        TestRequest request = new TestRequest { Value = "test" };

        await _logger.LogCommandAsync(typeof(TestRequest), request, null);
        await _logger.FlushAsync();

        File.Exists(_testLogPath).Should().BeTrue();
        string logContent = File.ReadAllText(_testLogPath);
        logContent.Should().Contain("Timestamp");
    }

    [Fact]
    public async Task FileEventBusLogger_LogCommandAsync_WithResultT_ShouldExtractValue()
    {
        TestRequest request = new TestRequest { Value = "test" };
        Result<TestResponse> result = Result.Success(new TestResponse { Result = "success-value" });

        await _logger.LogCommandAsync(typeof(TestRequest), request, result);
        await _logger.FlushAsync();

        File.Exists(_testLogPath).Should().BeTrue();
        string logContent = File.ReadAllText(_testLogPath);
        logContent.Should().Contain("success-value");
    }

    [Fact]
    public async Task FileEventBusLogger_LogCommandAsync_WithFailedResultT_ShouldNotExtractValue()
    {
        TestRequest request = new TestRequest { Value = "test" };
        Result<TestResponse> result = Result.Failure<TestResponse>(Error.Create("TestError", "Test error"));

        await _logger.LogCommandAsync(typeof(TestRequest), request, result);
        await _logger.FlushAsync();

        File.Exists(_testLogPath).Should().BeTrue();
        string logContent = File.ReadAllText(_testLogPath);
        logContent.Should().Contain("Command");
        logContent.Should().NotContain("success-value");
    }

    [Fact]
    public async Task FileEventBusLogger_LogStreamRequestAsync_ShouldCreateLogFile()
    {
        TestStreamRequest request = new TestStreamRequest { Value = "test" };
        TestResponse response = new TestResponse { Result = "stream-item" };

        await _logger.LogStreamRequestAsync(typeof(TestStreamRequest), request, response);
        await _logger.FlushAsync();

        File.Exists(_testLogPath).Should().BeTrue();
        string logContent = File.ReadAllText(_testLogPath);
        logContent.Should().Contain("StreamRequest");
        logContent.Should().Contain("TestStreamRequest");
        logContent.Should().Contain("stream-item");
    }

    [Fact]
    public async Task FileEventBusLogger_LogStreamRequestAsync_ShouldIncludeTimestamp()
    {
        TestStreamRequest request = new TestStreamRequest { Value = "test" };
        TestResponse response = new TestResponse { Result = "stream-item" };

        await _logger.LogStreamRequestAsync(typeof(TestStreamRequest), request, response);
        await _logger.FlushAsync();

        File.Exists(_testLogPath).Should().BeTrue();
        string logContent = File.ReadAllText(_testLogPath);
        logContent.Should().Contain("Timestamp");
    }

    [Fact]
    public async Task NullEventBusLogger_LogStreamRequestAsync_ShouldNotThrow()
    {
        NullMessagingLogger logger = new NullMessagingLogger();
        TestStreamRequest request = new TestStreamRequest { Value = "test" };
        TestResponse response = new TestResponse { Result = "stream-item" };

        Func<Task> act = async () => await logger.LogStreamRequestAsync(typeof(TestStreamRequest), request, response);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task FileEventBusLogger_LogStreamRequestAsync_WithResultT_ShouldExtractValue()
    {
        TestStreamRequest request = new TestStreamRequest { Value = "test" };
        Result<TestResponse> result = Result.Success(new TestResponse { Result = "stream-value" });

        await _logger.LogStreamRequestAsync(typeof(TestStreamRequest), request, result);
        await _logger.FlushAsync();

        File.Exists(_testLogPath).Should().BeTrue();
        string logContent = File.ReadAllText(_testLogPath);
        logContent.Should().Contain("stream-value");
    }

    [Fact]
    public async Task FileEventBusLogger_LogStreamRequestAsync_WithFailedResultT_ShouldNotExtractValue()
    {
        TestStreamRequest request = new TestStreamRequest { Value = "test" };
        Result<TestResponse> result = Result.Failure<TestResponse>(Error.Create("TestError", "Test error"));

        await _logger.LogStreamRequestAsync(typeof(TestStreamRequest), request, result);
        await _logger.FlushAsync();

        File.Exists(_testLogPath).Should().BeTrue();
        string logContent = File.ReadAllText(_testLogPath);
        logContent.Should().Contain("StreamRequest");
        logContent.Should().NotContain("stream-value");
    }

    private sealed class TestRequest
    {
        public string Value { get; set; } = string.Empty;
    }

    private sealed class TestResponse
    {
        public string Result { get; set; } = string.Empty;
    }

    private sealed class TestStreamRequest
    {
        public string Value { get; set; } = string.Empty;
    }
}
