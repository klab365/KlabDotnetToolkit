using Klab.Toolkit.Common;
using Microsoft.Extensions.Logging;

namespace Klab.Toolkit.Logging;

/// <summary>
/// Implementation of <see cref="ILoggerProvider"/> that publishes log messages.
/// /// </summary>
public sealed class LoggerPublisherProvider : ILoggerProvider
{
    private readonly ITimeProvider _timeProvider;
    private readonly ILoggerPublisherProxy _proxy;

    /// <summary>
    /// Create a new instance of the <see cref="LoggerPublisherProvider"/> class.
    /// </summary>
    /// <param name="timeProvider"></param>
    /// <param name="proxy"></param>
    public LoggerPublisherProvider(ITimeProvider timeProvider, ILoggerPublisherProxy proxy)
    {
        _timeProvider = timeProvider;
        _proxy = proxy;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        return new LoggerPublisher(_timeProvider, _proxy);
    }
}

/// <summary>
/// This class is used to publish log messages to the UI, or to a different location over the ILoggerProxy interface.
/// </summary>
public class LoggerPublisher : ILogger
{
    private readonly ITimeProvider _timeProvider;
    private readonly ILoggerPublisherProxy _proxy;

    /// <summary>
    /// Create a new instance of the <see cref="LoggerPublisher"/> class.
    /// </summary>
    /// <param name="timeProvider"></param>
    /// <param name="proxy"></param>
    public LoggerPublisher(ITimeProvider timeProvider, ILoggerPublisherProxy proxy)
    {
        _timeProvider = timeProvider;
        _proxy = proxy;
    }

    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        string exceptionStackTrace = exception != null && exception.StackTrace != null ? exception.StackTrace : "";
        LogData log = new(Date: _timeProvider.GetCurrentLocalTime().ToString(), Level: logLevel.ToString(), Message: exception?.Message ?? string.Empty, Exception: exceptionStackTrace);
        _proxy.PublishLog(log);
    }
}

/// <summary>
/// This class is used to publish log messages to the proxy.
/// </summary>
/// <param name="Date"></param>
/// <param name="Level"></param>
/// <param name="Message"></param>
/// <param name="Exception"></param>
/// <returns></returns>
public record LogData(string Date, string Level, string Message, string Exception);
