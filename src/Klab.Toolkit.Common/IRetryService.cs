using System;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;

namespace Klab.Toolkit.Common;

/// <summary>
/// Interface for a retry
/// </summary>
public interface IRetryService
{
    /// <summary>
    /// Try to call the given callback.
    ///
    /// If the callback throws an exception, it will be retried. If the callback
    /// succeeds, the result will be returned.
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="timeout"></param>
    /// <param name="retryCount"></param>
    /// <returns>result if the call was successful</returns>
    Task<Result> TryCallAsync(Func<CancellationToken, Task> callback, TimeSpan timeout, int retryCount = 3);
}
