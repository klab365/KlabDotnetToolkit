using System;

namespace Klab.Toolkit.Common;

/// <summary>
/// Abstract time related methods.
/// /// </summary>
public interface ITimeProvider
{
    /// <summary>
    /// Get the current time.
    /// </summary>
    /// <returns></returns>
    DateTimeOffset GetCurrentTime();
}
