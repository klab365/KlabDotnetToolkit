using System;
using System.Threading;
using System.Threading.Tasks;

namespace Klab.Toolkit.Common;

/// <summary>
/// Thread provider
/// This class provides differnt methos to work with threads or syncronization
/// </summary>
public sealed class TaskProvider : ITaskProvider
{
    private readonly object _syncLockObject = new();
    private TaskCompletionSource<bool> _syncTcs = new();
    private readonly SemaphoreSlim _ressourceLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskProvider"/> class.
    /// </summary>
    public TaskProvider()
    {
        _syncTcs.SetResult(false);
    }

    /// <inheritdoc/>
    public Task StartBackgroundJobAsync(Func<CancellationToken, Task> action, CancellationToken token)
    {
        return Task.Factory.StartNew(async () =>
        {
            try
            {
                await action(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation
            }
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
    }

    /// <inheritdoc/>
    public async Task DelayAsync(TimeSpan delay, CancellationToken token)
    {
        try
        {
            await Task.Delay(delay, token);
        }
        catch (TaskCanceledException)
        {
            // nothing needed here if the task is cancelled
        }
    }

    /// <inheritdoc/>
    public async Task LockAsync(CancellationToken token)
    {
        await _ressourceLock.WaitAsync(token).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task LockReleaseAsync()
    {
        _ressourceLock.Release();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async Task WaitForSyncReleaseAsync(CancellationToken token)
    {
        TaskCompletionSource<bool> tcs;

        lock (_syncLockObject)
        {
            if (_syncTcs.Task.IsCompleted)
            {
                _syncTcs = new TaskCompletionSource<bool>();
            }

            tcs = _syncTcs;
        }

        await tcs.Task.ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task ReleaseSyncLockAsync()
    {
        lock (_syncLockObject)
        {
            if (!_syncTcs.Task.IsCompleted)
            {
                _syncTcs.SetResult(true);
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _syncTcs.TrySetCanceled();
    }
}
