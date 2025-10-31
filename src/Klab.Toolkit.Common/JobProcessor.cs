using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Klab.Toolkit.Common;

/// <summary>
/// A generic job processor that processes jobs in a background worker.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class JobProcessor<T> : IDisposable, IJobProcessor<T>
{
    private readonly Channel<T> _channel;
    private Func<T, CancellationToken, Task>? _jobHandler; // Nullable until initialized
    private readonly CancellationTokenSource _cts = new();
    private Task? _workerTask;
    private readonly ILogger<JobProcessor<T>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobProcessor{T}"/> class.
    /// </summary>
    public JobProcessor(ILogger<JobProcessor<T>> logger)
    {
        _channel = Channel.CreateBounded<T>(1000);
        _logger = logger;
    }

    /// <summary>
    /// Initializes the job processor with a job handler.
    /// </summary>
    /// <param name="jobHandler"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Init(Func<T, CancellationToken, Task> jobHandler)
    {
        _jobHandler = jobHandler ?? throw new ArgumentNullException(nameof(jobHandler));
        _workerTask = Task.Run(ProcessJobsAsync);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }

    /// <inheritdoc/>
    public async Task EnqueueAsync(T job, CancellationToken cancellationToken = default)
    {
        if (_jobHandler == null)
        {
            _logger.LogError("JobProcessor received a job but has no handler set.");
            return;
        }

        try
        {
            await _channel.Writer.WriteAsync(job, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation exceptions
        }
    }

    /// <inheritdoc/>
    public async Task EnqueueAsync(IEnumerable<T> jobs, CancellationToken cancellationToken = default)
    {
        if (_jobHandler == null)
        {
            _logger.LogError("JobProcessor received jobs but has no handler set.");
            return;
        }

        foreach (T job in jobs)
        {
            try
            {
                await _channel.Writer.WriteAsync(job, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation exceptions
                break;
            }
        }
    }

    /// <inheritdoc/>
    public async Task StopAsync()
    {
        _channel.Writer.Complete();
        _cts.Cancel();

        if (_workerTask != null)
        {
            try
            {
                await _workerTask;
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation exceptions
            }
        }
    }

    private async Task ProcessJobsAsync()
    {
        await foreach (T job in _channel.Reader.ReadAllAsync(_cts.Token))
        {
            if (_jobHandler == null)
            {
                _logger.LogWarning("JobProcessor received a job but has no handler set.");
                continue;
            }

            try
            {
                await _jobHandler(job, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation exceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing a job.");
            }
        }
    }
}

