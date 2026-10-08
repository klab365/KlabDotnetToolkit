using System;
using System.Diagnostics.CodeAnalysis;
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
    [RequiresUnreferencedCode("Type-based queue and logger registration is not supported with trimming or Native AOT. Use the generic overload.")]
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

    /// <summary>
    /// Adds the messaging module with statically known queue and logger types.
    /// </summary>
    /// <typeparam name="TLogger">The messaging logger implementation.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional module configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddMessagingModule<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TLogger>(this IServiceCollection services, Action<MessagingModuleConfiguration>? configure = default)
        where TLogger : class, IMessagingLogger
    {
        MessagingModuleConfiguration configuration = new();
        configure?.Invoke(configuration);
        services.AddSingleton(configuration);
        services.AddLogging();
        services.AddSingleton<IEventQueue, InMemoryMessageQueue>();
        services.AddSingleton<TLogger>();
        services.AddSingleton<IMessagingLogger>(provider => provider.GetRequiredService<TLogger>());
        if (typeof(IHostedService).IsAssignableFrom(typeof(TLogger)))
        {
            services.AddSingleton<IHostedService>(provider => (IHostedService)provider.GetRequiredService<TLogger>());
        }

        services.AddSingleton<MessagingHandlerMediator>();
        services.AddSingleton<IMediator, Mediator>();
        services.AddHostedService<MessagingProcessorJob>();
        services.AddSingleton<IEventHandlerProcessingStrategy, TaskWhenAllPublisher>();
        return services;
    }

    [RequiresUnreferencedCode("Registering implementations by Type is not supported with trimming or Native AOT.")]
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

    [RequiresUnreferencedCode("Registering implementations by Type is not supported with trimming or Native AOT.")]
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
