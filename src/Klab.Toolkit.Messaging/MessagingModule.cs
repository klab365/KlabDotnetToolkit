using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Klab.Toolkit.Messaging;

/// <summary>
/// Messaging Module
/// </summary>
public static class MessagingModule
{
    /// <summary>
    /// Adds the messaging module to the service collection
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure" />
    /// <returns></returns>
    public static IServiceCollection AddMessagingModule(this IServiceCollection services, Action<MessagingModuleConfiguration>? configure = default)
    {
        MessagingModuleConfiguration configuration = new();
        configure?.Invoke(configuration);
        services.AddSingleton(configuration);

        RegisterEventQueue(services, configuration);
        RegisterMessagingLogger(services, configuration);
        services.AddSingleton<MessagingHandlerMediator>();
        services.AddSingleton<IMediator, Mediator>();
        services.AddHostedService<MessagingProcessorJob>();
        services.AddSingleton<IEventHandlerProcessingStrategy, TaskWhenAllPublisher>();
        return services;
    }

    private static void RegisterEventQueue(IServiceCollection services, MessagingModuleConfiguration configuration)
    {
        if (configuration.EventQueueType == null)
        {
            throw new InvalidOperationException("Event queue type is not set");
        }

        if (!typeof(IEventQueue).IsAssignableFrom(configuration.EventQueueType))
        {
            throw new ArgumentException("Invalid event queue type");
        }

        ServiceDescriptor eventQueueDescriptor = new(typeof(IEventQueue), configuration.EventQueueType, configuration.EventQueueLifetime);
        services.Add(eventQueueDescriptor);
    }

    private static void RegisterMessagingLogger(IServiceCollection services, MessagingModuleConfiguration configuration)
    {
        if (configuration.MessagingLoggerType == null)
        {
            throw new InvalidOperationException("Messaging logger type is not set");
        }

        if (!typeof(IMessagingLogger).IsAssignableFrom(configuration.MessagingLoggerType))
        {
            throw new ArgumentException("Invalid messaging logger type");
        }

        services.AddSingleton(typeof(IMessagingLogger), configuration.MessagingLoggerType);

        if (typeof(IHostedService).IsAssignableFrom(configuration.MessagingLoggerType))
        {
            services.AddSingleton(provider => (IHostedService)provider.GetRequiredService<IMessagingLogger>());
            services.AddHostedService(provider => (IHostedService)provider.GetRequiredService<IMessagingLogger>());
        }
    }
}
