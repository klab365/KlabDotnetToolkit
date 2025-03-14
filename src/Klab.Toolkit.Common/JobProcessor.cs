using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Klab.Toolkit.Common;

/// <summary>
/// A generic job processor that processes jobs in a background worker.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class JobProcessor<T> : IDisposable
{
    private readonly Channel<T> _channel;
    private Func<T, Task>? _jobHandler; // Nullable until initialized
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _workerTask;
    private readonly ILogger<JobProcessor<T>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobProcessor{T}"/> class.
    /// </summary>
    public JobProcessor(ILogger<JobProcessor<T>> logger)
    {
        _channel = Channel.CreateUnbounded<T>();
        _workerTask = Task.Run(ProcessJobsAsync);
        _logger = logger;
    }

    /// <summary>
    /// Initializes the job processor with a job handler.
    /// </summary>
    /// <param name="jobHandler"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Init(Func<T, Task> jobHandler)
    {
        _jobHandler = jobHandler ?? throw new ArgumentNullException(nameof(jobHandler));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    /// <summary>
    /// Enqueues a job to be processed.
    /// </summary>
    /// <param name="job"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task EnqueueAsync(T job)
    {
        if (_jobHandler == null)
        {
            _logger.LogError("JobProcessor received a job but has no handler set.");
            return;
        }

        await _channel.Writer.WriteAsync(job);
    }

    /// <summary>
    /// Stops the job processor.
    /// </summary>
    /// <returns></returns>
    public async Task StopAsync()
    {
        _channel.Writer.Complete();
        _cts.Cancel();
        await _workerTask;
    }

    private async Task ProcessJobsAsync()
    {
        await foreach (T job in _channel.Reader.ReadAllAsync(_cts.Token))
        {
            if (_jobHandler == null)
            {
                _logger.LogInformation("JobProcessor received a job but has no handler set.");
                continue;
            }

            try
            {
                await _jobHandler(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing a job.");
            }
        }
    }
}

