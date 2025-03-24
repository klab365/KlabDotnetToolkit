using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    private readonly EventModuleConfiguration _eventModuleConfiguration;
    private readonly List<object> _eventLogs = new();
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public EventProcesserJob(
        IEventBus eventBus,
        EventHandlerMediator eventHandlerMediator,
        ILogger<EventProcesserJob> logger,
        EventModuleConfiguration eventModuleConfiguration)
    {
        _eventBus = eventBus;
        _eventHandlerMediator = eventHandlerMediator;
        _logger = logger;
        _eventModuleConfiguration = eventModuleConfiguration;
        _jsonSerializerOptions = new()
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }, // Useful if you have enums
            PropertyNameCaseInsensitive = true,
        };
        _jsonSerializerOptions.Converters.Add(new EventInterfaceJsonConverter());
    }

    /// <summary>
    /// This method will process events sequentially from the event queue.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (IEvent @event in _eventBus.MessageQueue.DequeueAsync(stoppingToken))
        {
            try
            {
                Task<Result[]> task1 = ProcessHandlerClassesAsync(@event, stoppingToken);
                Task<Result[]> task2 = ProcessLocalFunctionsAsync(@event, stoppingToken);

                await Task.WhenAll(
                    task1,
                    task2
                );

                List<Result> res = [.. await task1, .. await task2];
                LogResult(@event, res.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the event {Event}", @event);
            }
        }
    }

    private void LogResult(IEvent @event, Result[] res)
    {
        if (!_eventModuleConfiguration.ShouldLogEvents)
        {
            return;
        }

        object eventLog = new {
            Event = @event,
            Results = GenerateResultLogs(res)
        };
        _eventLogs.Add(eventLog);
        string jsonLog = JsonSerializer.Serialize(_eventLogs, _jsonSerializerOptions);
        File.WriteAllText(_eventModuleConfiguration.EventLogFilePath, jsonLog);
    }

    private static IEnumerable<object> GenerateResultLogs(Result[] res)
    {
        if (res.All(r => r.IsSuccess))
        {
            return [];
        }

        IEnumerable<object> failedResults = res
            .Where(r => !r.IsSuccess)
            .Select(r => new { r.Error });

        return failedResults;
    }

    private async Task<Result[]> ProcessHandlerClassesAsync(IEvent @event, CancellationToken stoppingToken)
    {
        Result[] res = await _eventHandlerMediator.PublishToHandlersAsync(@event, stoppingToken);
        return res;
    }

    private async Task<Result[]> ProcessLocalFunctionsAsync(IEvent @event, CancellationToken stoppingToken)
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
