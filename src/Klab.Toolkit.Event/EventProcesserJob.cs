using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            try
            {
                await Task.WhenAll(
                    ProcessHandlerClassesAsync(@event, stoppingToken),
                    ProcessLocalFunctionsAsync(@event, stoppingToken)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the event {Event}", @event);
            }
        }
    }

    private async Task ProcessHandlerClassesAsync(IEvent @event, CancellationToken stoppingToken)
    {
        try
        {
            await _eventHandlerMediator.PublishAsync(@event, stoppingToken);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogDebug(ex, "No event handler found for event type {EventType}", @event.GetType());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the event {Event} with the handler classes", @event);
        }
    }

    private async Task ProcessLocalFunctionsAsync(IEvent @event, CancellationToken stoppingToken)
    {
        if (!_eventBus.LocalEventHandlers.ContainsKey(@event.GetType()))
        {
            return;
        }

        IEnumerable<Task> tasks = _eventBus
            .LocalEventHandlers[@event.GetType()]
            .Select(handler => handler.Value(@event, stoppingToken));

        await Task.WhenAll(tasks);
    }
}
