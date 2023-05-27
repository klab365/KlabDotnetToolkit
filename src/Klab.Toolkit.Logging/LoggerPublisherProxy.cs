namespace Klab.Toolkit.Logging;

/// <summary>
/// Interface for the UiLoggerProxy
/// </summary>
public interface ILoggerPublisherProxy
{
    /// <summary>
    /// Raised when the log is published.
    /// </summary>
    event EventHandler<LogData> PublishedLog;

    /// <summary>
    /// Publish the log.
    /// </summary>
    /// <param name="log"></param>
    void PublishLog(LogData log);
}

/// <summary>
/// This class is a proxy to pass the entered log to other components (e.g. SysLogControlViewModel)
/// </summary>
public class LoggerPublisherProxy : ILoggerPublisherProxy
{
    /// <inheritdoc/>
    public event EventHandler<LogData>? PublishedLog;

    /// <inheritdoc/>
    public void PublishLog(LogData log)
    {
        PublishedLog?.Invoke(this, log);
    }
}
