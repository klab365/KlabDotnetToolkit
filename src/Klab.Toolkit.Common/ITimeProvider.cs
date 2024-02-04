using System;

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
    DateTimeOffset GetCurrentTime();
}
