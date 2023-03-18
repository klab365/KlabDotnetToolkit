namespace Klab.Toolkit.Common;

/// <summary>
/// Abstract time related methods.
/// /// </summary>
public interface ITimeProvider
{
    /// <summary>
    /// String Format for DateTime
    /// </summary>
    /// <value></value>
    string DataTimeFormat { get; }

    /// <summary>
    /// Get the current time.
    /// </summary>
    /// <returns></returns>
    DateTime GetCurrentTime();

    /// <summary>
    /// Get the current time in UTC.
    /// </summary>
    /// <returns></returns>
    DateTime GetCurrentLocalTime();

    /// <summary>
    /// Format the given time in the format.
    /// </summary>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    string ToString(DateTime dateTime);

    /// <summary>
    /// Sleep for the given time.
    /// </summary>
    /// <param name="time"></param>
    void Sleep(TimeSpan time);
}

/// <summary>
/// Implementation of <see cref="ITimeProvider"/> that uses the system clock.
/// </summary>
public class TimeProvider : ITimeProvider
{
    /// <inheritdoc/>
    public string DataTimeFormat { get; }

    /// <summary>
    /// Create a new instance of <see cref="TimeProvider"/>.
    /// </summary>
    /// <param name="dateTimeFormat"></param>
    public TimeProvider(string dateTimeFormat)
    {
        DataTimeFormat = dateTimeFormat;
    }

    /// <inheritdoc/>
    public DateTime GetCurrentLocalTime()
    {
        return DateTime.Now;
    }

    /// <inheritdoc/>
    public DateTime GetCurrentTime()
    {
        return DateTime.UtcNow;
    }

    /// <inheritdoc/>
    public string ToString(DateTime dateTime)
    {
        return dateTime.ToString(DataTimeFormat);
    }

    /// <inheritdoc/>
    public void Sleep(TimeSpan time)
    {
        Task.Delay(time).Wait();
    }
}
