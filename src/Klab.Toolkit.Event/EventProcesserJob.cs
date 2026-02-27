using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Klab.Toolkit.Event;

/// <summary>
/// Background service that processes events from the event queue and
/// call the mediator to publish the event (mediator-based event bus)
///
/// It uses an <see cref="IEventQueue"/> to dequeue events. The EventQueue can
/// be implemented using a message broker like RabbitMQ, Azure Service Bus, or a simple in-memory queue.
/// The processer should dequeue the events and call the mediator to publish the event.
/// </summary>
internal sealed class EventProcesserJob : BackgroundService
{
    private readonly IEventBus _eventBus;
    private readonly EventHandlerMediator _eventHandlerMediator;
    private readonly ILogger<EventProcesserJob> _logger;
    private readonly ICqrsEventLogger? _cqrsEventLogger;

    public EventProcesserJob(
        IEventBus eventBus,
        EventHandlerMediator eventHandlerMediator,
        ILogger<EventProcesserJob> logger,
        ICqrsEventLogger? cqrsEventLogger = null)
    {
        _eventBus = eventBus;
        _eventHandlerMediator = eventHandlerMediator;
        _logger = logger;
        _cqrsEventLogger = cqrsEventLogger;
    }

    /// <summary>
    /// This method will process events sequentially from the event queue.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (EventBase @event in _eventBus.MessageQueue.DequeueAsync(stoppingToken))
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
                LogResult(@event, [.. res]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the event {Event}", @event);
            }
        }
    }

    private void LogResult(EventBase @event, Result[] res)
    {
        _cqrsEventLogger?.LogProcessedEvent(@event, res);
    }

    private async Task<Result[]> ProcessHandlerClassesAsync(EventBase @event, CancellationToken stoppingToken)
    {
        Result[] res = await _eventHandlerMediator.PublishToHandlersAsync(@event, stoppingToken);
        return res;
    }

    private async Task<Result[]> ProcessLocalFunctionsAsync(EventBase @event, CancellationToken stoppingToken)
    {
        if (!_eventBus.GetLocalEventHandlers().ContainsKey(@event.GetType()))
        {
            return [];
        }

        IEnumerable<Task<Result>> tasks = _eventBus
            .GetLocalEventHandlers()[@event.GetType()]
            .Select(handler => handler.Value(@event, stoppingToken));

        Result[] res = await Task.WhenAll(tasks);
        return res;
    }
}
