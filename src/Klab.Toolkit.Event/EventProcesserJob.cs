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

    public EventProcesserJob(
        IEventBus eventBus,
        EventHandlerMediator eventHandlerMediator,
        ILogger<EventProcesserJob> logger)
    {
        _eventBus = eventBus;
        _eventHandlerMediator = eventHandlerMediator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (IEvent @event in _eventBus.MessageQueue.DequeueAsync(stoppingToken))
        {
            Task task = new(async () =>
            {
                try
                {
                    Task<IResult[]> task1 = ProcessHandlerClassesAsync(@event, stoppingToken);
                    Task<IResult[]> task2 = ProcessLocalFunctionsAsync(@event, stoppingToken);

                    await Task.WhenAll(
                        task1,
                        task2
                    );

                    List<IResult> res = [.. await task1, .. await task2];
                    LogResult(@event, res.ToArray());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing the event {Event}", @event);
                }
            }, stoppingToken);
            task.Start();
        }
    }

    private void LogResult(IEvent @event, IResult[] res)
    {
        if (!res.Any(r => r.IsFailure))
        {
            return;
        }

        foreach (IResult r in res)
        {
            if (r.IsFailure)
            {
                _logger.LogError("An error occurred while processing the event {Event}: {Error}", @event, r.Error);
            }
        }
    }

    private async Task<IResult[]> ProcessHandlerClassesAsync(IEvent @event, CancellationToken stoppingToken)
    {
        IResult[] res = await _eventHandlerMediator.PublishToHandlersAsync(@event, stoppingToken);
        return res;
    }

    private async Task<IResult[]> ProcessLocalFunctionsAsync(IEvent @event, CancellationToken stoppingToken)
    {
        if (!_eventBus.GetLocalEventHandlers().ContainsKey(@event.GetType()))
        {
            return [];
        }

        IEnumerable<Task<IResult>> tasks = _eventBus
            .GetLocalEventHandlers()[@event.GetType()]
            .Select(handler => handler.Value(@event, stoppingToken));

        IResult[] res = await Task.WhenAll(tasks);
        return res;
    }
}
