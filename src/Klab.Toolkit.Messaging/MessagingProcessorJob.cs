using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Klab.Toolkit.Messaging;

/// <summary>
/// Background service that processes events from the event queue and
/// call the mediator to publish the event (mediator-based messaging)
///
/// It uses an <see cref="IEventQueue"/> to dequeue events. The EventQueue can
/// be implemented using a message broker like RabbitMQ, Azure Service Bus, or a simple in-memory queue.
/// The processer should dequeue the events and call the mediator to publish the event.
/// </summary>
internal sealed class MessagingProcessorJob : BackgroundService
{
    private readonly IMediator _mediator;
    private readonly MessagingHandlerMediator _messagingHandlerMediator;
    private readonly ILogger<MessagingProcessorJob> _logger;
    private readonly IMessagingLogger _messagingLogger;

    public MessagingProcessorJob(
        IMediator mediator,
        MessagingHandlerMediator messagingHandlerMediator,
        ILogger<MessagingProcessorJob> logger,
        IMessagingLogger messagingLogger)
    {
        _mediator = mediator;
        _messagingHandlerMediator = messagingHandlerMediator;
        _logger = logger;
        _messagingLogger = messagingLogger;
    }

    /// <summary>
    /// This method will process events sequentially from the event queue.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (EventBase @event in _mediator.MessageQueue.DequeueAsync(stoppingToken))
        {
            try
            {
                Task<Result[]> task1 = ProcessHandlerClassesAsync(@event, stoppingToken);
                Task<Result[]> task2 = ProcessLocalFunctionsAsync(@event, stoppingToken);

                await Task.WhenAll(
                    task1,
                    task2
                );

                List<Result> res = [.. task1.Result, .. task2.Result];
                await _messagingLogger.LogEventAsync(@event, [.. res]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the event {Event}", @event);
            }
        }
    }

    private async Task<Result[]> ProcessHandlerClassesAsync(EventBase @event, CancellationToken stoppingToken)
    {
        Result[] res = await _messagingHandlerMediator.PublishToHandlersAsync(@event, stoppingToken);
        return res;
    }

    private async Task<Result[]> ProcessLocalFunctionsAsync(EventBase @event, CancellationToken stoppingToken)
    {
        if (!_mediator.GetLocalEventHandlers().ContainsKey(@event.GetType()))
        {
            return [];
        }

        IEnumerable<Task<Result>> tasks = _mediator
            .GetLocalEventHandlers()[@event.GetType()]
            .Select(handler => handler.Value(@event, stoppingToken));

        Result[] res = await Task.WhenAll(tasks);
        return res;
    }
}
