using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
    public static IServiceCollection AddEventModule(this IServiceCollection services, Action<EventModuleConfiguration>? configure = default)
    {
        EventModuleConfiguration configuration = new EventModuleConfiguration();
        configure?.Invoke(configuration);
        services.AddSingleton(configuration);

        RegisterEventQueue(services, configuration);
        RegisterEventBusLogger(services, configuration);
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

    private static void RegisterEventBusLogger(IServiceCollection services, EventModuleConfiguration configuration)
    {
        if (configuration.EventBusLoggerType != null)
        {
            if (!typeof(IEventBusLogger).IsAssignableFrom(configuration.EventBusLoggerType))
            {
                throw new ArgumentException("Invalid event bus logger type");
            }

            services.AddSingleton(typeof(IEventBusLogger), configuration.EventBusLoggerType);

            if (typeof(IHostedService).IsAssignableFrom(configuration.EventBusLoggerType))
            {
                services.AddSingleton(provider => (IHostedService)provider.GetRequiredService<IEventBusLogger>());
                services.AddHostedService(provider => (IHostedService)provider.GetRequiredService<IEventBusLogger>());
            }
        }
        else
        {
            services.AddSingleton<FileEventBusLogger>();
            services.AddSingleton<IEventBusLogger>(provider => provider.GetRequiredService<FileEventBusLogger>());
            services.AddHostedService(provider => provider.GetRequiredService<FileEventBusLogger>());
        }
    }
}
