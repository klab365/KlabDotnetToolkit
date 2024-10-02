using System;
using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.Event;

/// <summary>
/// Event Module
/// </summary>
public static class EventModule
{
    /// <summary>
    /// Adds the event module to the service collection
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure" />
    /// <returns></returns>
    public static IServiceCollection UseEventModule(this IServiceCollection services, Action<EventModuleConfiguration>? configure = default)
    {
        EventModuleConfiguration configuration = new EventModuleConfiguration();
        configure?.Invoke(configuration);

        RegisterEventQueue(services, configuration);
        services.AddSingleton<EventHandlerMediator>();
        services.AddSingleton<IEventBus, EventBus>();
        services.AddHostedService<EventProcesserJob>();
        services.AddTransient<IEventHandlerProcessingStrategy, TaskWhenAllPublisher>();
        return services;
    }

    private static void RegisterEventQueue(IServiceCollection services, EventModuleConfiguration eventModuleConfiguration)
    {
        if (eventModuleConfiguration.EventQueueType == null)
        {
            throw new InvalidOperationException("Event queue type is not set");
        }

        if (!typeof(IEventQueue).IsAssignableFrom(eventModuleConfiguration.EventQueueType))
        {
            throw new ArgumentException("Invalid event queue type");
        }

        ServiceDescriptor eventQueueDescriptor = new(typeof(IEventQueue), eventModuleConfiguration.EventQueueType, eventModuleConfiguration.EventQueueLifetime);
        services.Add(eventQueueDescriptor);
    }


}

/// <summary>
/// Event Module Configuration
/// </summary>
public class EventModuleConfiguration
{
    /// <summary>
    /// Gets or sets the event queue type
    /// </summary>
    public Type? EventQueueType { get; set; } = typeof(InMemoryMessageQueue);

    /// <summary>
    /// Gets or sets the event queue lifetime
    /// </summary>
    public ServiceLifetime EventQueueLifetime { get; set; } = ServiceLifetime.Singleton;
}
