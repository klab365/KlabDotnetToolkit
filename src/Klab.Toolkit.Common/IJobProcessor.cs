using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Klab.Toolkit.Common;

/// <summary>
/// Interface for a generic job processor that processes jobs in a background worker.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IJobProcessor<T>
{
    /// <summary>
    /// Enqueues a job to be processed.
    /// </summary>
    /// <returns></returns>
    Task EnqueueAsync(T job, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enqueues multiple jobs to be processed.
    /// </summary>
    /// <returns></returns>
    Task EnqueueAsync(IEnumerable<T> jobs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes the job processor with a job handler.
    /// </summary>
    /// <param name="jobHandler"></param>
    void Init(Func<T, CancellationToken, Task> jobHandler);

    /// <summary>
    /// Stops the job processor.
    /// </summary>
    /// <returns></returns>
    Task StopAsync();
}

