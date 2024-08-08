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
    public static IServiceCollection AddEventModule(this IServiceCollection services, Action<EventModuleConfiguration> configure)
    {
        EventModuleConfiguration configuration = new EventModuleConfiguration();
        configure(configuration);

        RegisterEventQueue(services, configuration);
        services.AddSingleton<EventHandlerMediator>();
        services.AddSingleton<IEventBus, EventBus>();
        services.AddHostedService<EventProcesserJob>();
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

    /// <summary>
    /// Adds the event subscription
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="THandler"></typeparam>
    /// <param name="services"></param>
    /// <param name="lifetime"></param>
    public static void AddEventSubsribtion<TEvent, THandler>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TEvent : IEvent
        where THandler : class, IEventHandler<TEvent>
    {
        if (lifetime == ServiceLifetime.Singleton)
        {
            services.AddSingleton<THandler>();
            services.AddSingleton<IEventHandler<TEvent>, THandler>(p => p.GetRequiredService<THandler>());
        }
        else if (lifetime == ServiceLifetime.Scoped)
        {
            services.AddScoped<THandler>();
            services.AddScoped<IEventHandler<TEvent>, THandler>(p => p.GetRequiredService<THandler>());
        }
        else if (lifetime == ServiceLifetime.Transient)
        {
            services.AddTransient<THandler>();
            services.AddTransient<IEventHandler<TEvent>, THandler>(p => p.GetRequiredService<THandler>());
        }
        else
        {
            throw new ArgumentException("Invalid lifetime", nameof(lifetime));
        }

        services.AddTransient<EventHandlerWrapper<TEvent>>();
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
    public Type? EventQueueType { get; set; }

    /// <summary>
    /// Gets or sets the event queue lifetime
    /// </summary>
    public ServiceLifetime EventQueueLifetime { get; set; } = ServiceLifetime.Singleton;
}
