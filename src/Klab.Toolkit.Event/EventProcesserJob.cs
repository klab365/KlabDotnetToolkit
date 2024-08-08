using System;
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
    private readonly IEventQueue _messageQueue;
    private readonly EventHandlerMediator _eventHandlerMediator;
    private readonly ILogger<EventProcesserJob> _logger;

    public EventProcesserJob(
        IEventQueue messageQueue,
        EventHandlerMediator eventHandlerMediator,
        ILogger<EventProcesserJob> logger)
    {
        _messageQueue = messageQueue;
        _eventHandlerMediator = eventHandlerMediator;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (IEvent @event in _messageQueue.DequeueAsync(stoppingToken))
        {
            try
            {
                await _eventHandlerMediator.PublishAsync(@event, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the event {Event}", @event);
            }
        }
    }
}
