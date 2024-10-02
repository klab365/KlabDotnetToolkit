using System;
using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.Event;

/// <summary>
/// Dependency Injection Extensions for Event Module
/// </summary>
public static class DependencyInjection
{
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
    /// Adds the request handler with response
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="THandler"></typeparam>
    /// <param name="services"></param>
    /// <param name="lifetime"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void AddRequestResponseHandler<TRequest, TResponse, THandler>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TRequest : IRequest<TResponse>
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

        services.AddTransient<RequestResponseHandlerWrapper<TRequest, TResponse>>();
    }

    /// <summary>
    /// Add stream request response handler to the service collection
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <typeparam name="THandler"></typeparam>
    /// <param name="services"></param>
    /// <param name="lifetime"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void AddStreamRequestResponseHandler<TRequest, TResponse, THandler>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TRequest : IStreamRequest<TResponse>
        where TResponse : notnull
        where THandler : class, IStreamRequestHandler<TRequest, TResponse>
    {
        if (lifetime == ServiceLifetime.Singleton)
        {
            services.AddSingleton<THandler>();
            services.AddSingleton<IStreamRequestHandler<TRequest, TResponse>, THandler>(p => p.GetRequiredService<THandler>());
        }
        else if (lifetime == ServiceLifetime.Scoped)
        {
            services.AddScoped<THandler>();
            services.AddScoped<IStreamRequestHandler<TRequest, TResponse>, THandler>(p => p.GetRequiredService<THandler>());
        }
        else if (lifetime == ServiceLifetime.Transient)
        {
            services.AddTransient<THandler>();
            services.AddTransient<IStreamRequestHandler<TRequest, TResponse>, THandler>(p => p.GetRequiredService<THandler>());
        }
        else
        {
            throw new ArgumentException("Invalid lifetime", nameof(lifetime));
        }

        services.AddTransient<StreamRequestResponseHandlerWrapper<TRequest, TResponse>>();
    }
}
