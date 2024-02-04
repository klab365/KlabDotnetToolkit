using System;

namespace Klab.Toolkit.Common;

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
    public DateTimeOffset GetCurrentTime()
    {
        return new DateTimeOffset(DateTime.UtcNow);
    }
}
