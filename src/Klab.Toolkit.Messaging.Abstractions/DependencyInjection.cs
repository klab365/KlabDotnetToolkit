using System;
using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.Messaging;

/// <summary>
/// Dependency Injection Extensions for Messaging Module
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds an event handler
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="THandler"></typeparam>
    /// <param name="services"></param>
    /// <param name="lifetime"></param>
    public static void AddEventHandler<TEvent, THandler>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TEvent : EventBase
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
    /// Adds a request middleware to the pipeline for the specified request and response types.
    /// Middleware is executed in registration order, outermost first.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <typeparam name="TMiddleware">The middleware implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The DI lifetime of the middleware. Defaults to Transient.</param>
    public static void AddRequestMiddleware<TRequest, TResponse, TMiddleware>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TRequest : IRequest<TResponse>
        where TResponse : notnull
        where TMiddleware : class, IRequestMiddleware<TRequest, TResponse>
    {
        ServiceDescriptor descriptor = new(typeof(IRequestMiddleware<TRequest, TResponse>), typeof(TMiddleware), lifetime);
        services.Add(descriptor);
    }

    /// <summary>
    /// Adds a global open generic request middleware that applies to all request/response pairs.
    /// The middleware type must be an open generic type definition with exactly two parameters
    /// (e.g. <c>typeof(MyMiddleware&lt;,&gt;)</c>) that implements <see cref="IRequestMiddleware{TRequest,TResponse}"/>.
    /// When registered as Singleton, the same instance is shared for all closed generic resolutions.
    /// Middleware is executed in registration order, outermost first.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="middlewareType">The open generic middleware implementation type.</param>
    /// <param name="lifetime">The DI lifetime of the middleware. Defaults to Singleton.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddGlobalRequestMiddleware(this IServiceCollection services, Type middlewareType, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (middlewareType is null)
        {
            throw new ArgumentNullException(nameof(middlewareType));
        }

        if (!middlewareType.IsGenericTypeDefinition)
        {
            throw new ArgumentException("Middleware type must be an open generic type definition (e.g. typeof(MyMiddleware<,>)).", nameof(middlewareType));
        }

        if (middlewareType.GetGenericArguments().Length != 2)
        {
            throw new ArgumentException("Middleware type must define exactly two generic parameters.", nameof(middlewareType));
        }

        if (middlewareType.IsInterface || middlewareType.IsAbstract)
        {
            throw new ArgumentException("Middleware type must be a concrete class.", nameof(middlewareType));
        }

        bool implementsInterface = false;
        foreach (Type i in middlewareType.GetInterfaces())
        {
            if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestMiddleware<,>))
            {
                implementsInterface = true;
                break;
            }
        }

        if (!implementsInterface)
        {
            throw new ArgumentException(
                $"'{middlewareType.Name}' must implement '{typeof(IRequestMiddleware<,>).Name}'.",
                nameof(middlewareType));
        }

        services.Add(new ServiceDescriptor(typeof(IRequestMiddleware<,>), middlewareType, lifetime));
        return services;
    }

    /// <summary>
    /// Adds a stream request middleware to the pipeline for the specified stream request and response types.
    /// Middleware is executed in registration order, outermost first.
    /// </summary>
    /// <typeparam name="TRequest">The stream request type.</typeparam>
    /// <typeparam name="TResponse">The item type yielded by the stream.</typeparam>
    /// <typeparam name="TMiddleware">The middleware implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The DI lifetime of the middleware. Defaults to Transient.</param>
    public static void AddStreamRequestMiddleware<TRequest, TResponse, TMiddleware>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TRequest : IStreamRequest<TResponse>
        where TResponse : notnull
        where TMiddleware : class, IStreamRequestMiddleware<TRequest, TResponse>
    {
        ServiceDescriptor descriptor = new(typeof(IStreamRequestMiddleware<TRequest, TResponse>), typeof(TMiddleware), lifetime);
        services.Add(descriptor);
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
