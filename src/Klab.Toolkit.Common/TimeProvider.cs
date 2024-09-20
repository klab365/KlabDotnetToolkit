using System;

namespace Klab.Toolkit.Common;

/// <summary>
/// Implementation of <see cref="ITimeProvider"/> that uses the system clock.
/// </summary>
public class TimeProvider : ITimeProvider
{
    /// <inheritdoc />
    public DateTimeOffset GetCurrentTime()
    {
        return new DateTimeOffset(DateTime.UtcNow);
    }
}
