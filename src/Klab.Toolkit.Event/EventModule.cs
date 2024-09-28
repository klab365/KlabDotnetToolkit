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
    public static IServiceCollection UseEventModule(this IServiceCollection services, Action<EventModuleConfiguration>? configure)
    {
        EventModuleConfiguration configuration = new EventModuleConfiguration();
        configure?.Invoke(configuration);

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

    /// <summary>
    /// Adds the request handler without response
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="THandler"></typeparam>
    /// <param name="services"></param>
    /// <param name="lifetime"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void AddRequestHandler<TRequest, THandler>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TRequest : IRequest
        where THandler : class, IRequestHandler<TRequest>
    {
        if (lifetime == ServiceLifetime.Singleton)
        {
            services.AddSingleton<THandler>();
            services.AddSingleton<IRequestHandler<TRequest>, THandler>(p => p.GetRequiredService<THandler>());
        }
        else if (lifetime == ServiceLifetime.Scoped)
        {
            services.AddScoped<THandler>();
            services.AddScoped<IRequestHandler<TRequest>, THandler>(p => p.GetRequiredService<THandler>());
        }
        else if (lifetime == ServiceLifetime.Transient)
        {
            services.AddTransient<THandler>();
            services.AddTransient<IRequestHandler<TRequest>, THandler>(p => p.GetRequiredService<THandler>());
        }
        else
        {
            throw new ArgumentException("Invalid lifetime", nameof(lifetime));
        }
    }

    /// <summary>
    /// Adds the request handler with response
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="THandler"></typeparam>
    /// <param name="services"></param>
    /// <param name="lifetime"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void AddRequestResponseHandler<TRequest, TResponse, THandler>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TRequest : IRequest
        where TResponse : notnull
        where THandler : class, IRequestHandler<TRequest, TResponse>
    {
        if (lifetime == ServiceLifetime.Singleton)
        {
            services.AddSingleton<THandler>();
            services.AddSingleton<IRequestHandler<TRequest, TResponse>, THandler>(p => p.GetRequiredService<THandler>());
        }
        else if (lifetime == ServiceLifetime.Scoped)
        {
            services.AddScoped<THandler>();
            services.AddScoped<IRequestHandler<TRequest, TResponse>, THandler>(p => p.GetRequiredService<THandler>());
        }
        else if (lifetime == ServiceLifetime.Transient)
        {
            services.AddTransient<THandler>();
            services.AddTransient<IRequestHandler<TRequest, TResponse>, THandler>(p => p.GetRequiredService<THandler>());
        }
        else
        {
            throw new ArgumentException("Invalid lifetime", nameof(lifetime));
        }
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
