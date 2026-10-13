using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Klab.Toolkit.Messaging;

internal sealed class MessagingProcessorJob : BackgroundService
{
    private readonly IMediator _mediator;
    private readonly MessagingHandlerMediator _messagingHandlerMediator;
    private readonly ILogger<MessagingProcessorJob> _logger;
    private readonly IMessagingLogger _messagingLogger;
    private readonly MessagingModuleConfiguration _configuration;
    private readonly ConcurrentDictionary<string, byte> _processedIdempotencyKeys = new();

    public MessagingProcessorJob(
        IMediator mediator,
        MessagingHandlerMediator messagingHandlerMediator,
        ILogger<MessagingProcessorJob> logger,
        IMessagingLogger messagingLogger,
        MessagingModuleConfiguration configuration)
    {
        _mediator = mediator;
        _messagingHandlerMediator = messagingHandlerMediator;
        _logger = logger;
        _messagingLogger = messagingLogger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (EventBase @event in _mediator.MessageQueue.DequeueAsync(stoppingToken))
        {
            if (_configuration.EnableIdempotency && _processedIdempotencyKeys.ContainsKey(@event.IdempotencyKey))
            {
                continue;
            }

            try
            {
                await ProcessWithRetryAsync(@event, stoppingToken);
                _processedIdempotencyKeys.TryAdd(@event.IdempotencyKey, 0);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the event {Event}", @event);
                await DeadLetterAsync(@event, ex, stoppingToken);
            }
        }
    }

    private async Task ProcessWithRetryAsync(EventBase @event, CancellationToken stoppingToken)
    {
        int attemptCount = _configuration.EventProcessingRetryCount + 1;
        Exception? lastException = null;

        for (int attempt = 1; attempt <= attemptCount; attempt++)
        {
            try
            {
                Result[] results = await ProcessOnceAsync(@event, stoppingToken);
                Result? failure = results.FirstOrDefault(result => result.IsFailure);
                if (failure is null)
                {
                    await _messagingLogger.LogEventAsync(@event, results);
                    return;
                }

                lastException = new InvalidOperationException(failure.Error.Message, failure.Error.Exception);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;
            }

            if (attempt < attemptCount)
            {
                TimeSpan delay = GetRetryDelay(attempt);
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, stoppingToken);
                }
            }
        }

        throw lastException ?? new InvalidOperationException("Event processing failed.");
    }

    private async Task<Result[]> ProcessOnceAsync(EventBase @event, CancellationToken stoppingToken)
    {
        using CancellationTokenSource? timeoutCancellationTokenSource = _configuration.EventProcessingTimeout is TimeSpan timeout
            ? new CancellationTokenSource(timeout)
            : null;
        using CancellationTokenSource? processingCancellationTokenSource = timeoutCancellationTokenSource is null
            ? null
            : CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeoutCancellationTokenSource.Token);
        CancellationToken processingToken = processingCancellationTokenSource?.Token ?? stoppingToken;

        Task<Result[]> handlerTask = ProcessHandlerClassesAsync(@event, processingToken);
        Task<Result[]> localFunctionTask = ProcessLocalFunctionsAsync(@event, processingToken);
        Task allTasks = Task.WhenAll(handlerTask, localFunctionTask);

        if (timeoutCancellationTokenSource is not null)
        {
            Task completedTask = await Task.WhenAny(allTasks, Task.Delay(Timeout.InfiniteTimeSpan, processingToken));
            stoppingToken.ThrowIfCancellationRequested();
            if (timeoutCancellationTokenSource.IsCancellationRequested || completedTask != allTasks)
            {
                throw new TimeoutException($"Event processing exceeded the configured timeout of {_configuration.EventProcessingTimeout}.");
            }
        }

        await allTasks;
        return [.. handlerTask.Result, .. localFunctionTask.Result];
    }

    private async Task DeadLetterAsync(EventBase @event, Exception exception, CancellationToken stoppingToken)
    {
        if (_mediator.MessageQueue is not IDeadLetterEventQueue deadLetterQueue || stoppingToken.IsCancellationRequested)
        {
            return;
        }

        try
        {
            await deadLetterQueue.DeadLetterAsync(
                @event,
                new EventProcessingFailure(_configuration.EventProcessingRetryCount + 1, exception),
                stoppingToken);
        }
        catch (Exception deadLetterException)
        {
            _logger.LogError(deadLetterException, "An error occurred while dead-lettering the event {Event}", @event);
        }
    }

    private TimeSpan GetRetryDelay(int failedAttempt)
    {
        double milliseconds = _configuration.EventProcessingRetryDelay.TotalMilliseconds * Math.Pow(_configuration.EventProcessingRetryBackoffFactor, failedAttempt - 1);
        return milliseconds >= TimeSpan.MaxValue.TotalMilliseconds
            ? TimeSpan.MaxValue
            : TimeSpan.FromMilliseconds(milliseconds);
    }

    private async Task<Result[]> ProcessHandlerClassesAsync(EventBase @event, CancellationToken stoppingToken)
    {
        return await _messagingHandlerMediator.PublishToHandlersAsync(@event, stoppingToken);
    }

    private async Task<Result[]> ProcessLocalFunctionsAsync(EventBase @event, CancellationToken stoppingToken)
    {
        if (_mediator is not Mediator mediator || !mediator.TryGetWrappers(@event.GetType(), out IEnumerable<Func<EventBase, CancellationToken, Task<Result>>> wrappers))
        {
            return [];
        }

        return await Task.WhenAll(wrappers.Select(wrapper => wrapper(@event, stoppingToken)));
    }
}
