using System;
using System.Threading;
using System.Threading.Tasks;

namespace Klab.Toolkit.Common;

/// <summary>
/// Interface for providing thread related operations
/// </summary>
public interface ITaskProvider : IDisposable
{
    /// <summary>
    /// Delay the task
    /// </summary>
    Task DelayAsync(TimeSpan delay, CancellationToken token);

    /// <summary>
    /// Lock the resource
    /// </summary>
    Task LockAsync(CancellationToken token);

    /// <summary>
    /// Release the lock
    /// </summary>
    Task LockReleaseAsync();

    /// <summary>
    /// Release the sync lock
    /// </summary>
    Task ReleaseSyncLockAsync();

    /// <summary>
    /// Wait for sync release
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task WaitForSyncReleaseAsync(CancellationToken token);

    /// <summary>
    /// Start a background job
    /// </summary>
    Task StartBackgroundJobAsync(Func<CancellationToken, Task> action, CancellationToken token);
}
