using System;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;

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

    /// <summary>
    /// Wait for the specified time span.
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Result> WaitAsync(TimeSpan timeSpan, CancellationToken cancellationToken = default);
}
