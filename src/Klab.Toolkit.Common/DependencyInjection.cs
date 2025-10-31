using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.Common;

/// <summary>
/// Dependency injection extensions for the Klab.Toolkit.Common library.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    /// <summary>
    /// Add all services to the service collection.
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AddKlabToolkitCommon(IServiceCollection services)
    {
        services.AddRetryService();
        services.AddTimeProvider();
        services.AddTransient<ITaskProvider, TaskProvider>();
        return services;
    }

    /// <summary>
    /// Add <see cref="IRetryService"/> to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddRetryService(this IServiceCollection services)
    {
        services.AddTransient<IRetryService, RetryService>();
        return services;
    }

    /// <summary>
    /// Add <see cref="ITimeProvider"/> to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddTimeProvider(this IServiceCollection services)
    {
        services.AddTransient<ITimeProvider, TimeProvider>();
        return services;
    }

    /// <summary>
    /// Add <see cref="IJobProcessor{T}"/> to the service collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <param name="lifetime"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static IServiceCollection AddJobProcessor<T>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                services.AddSingleton<IJobProcessor<T>, JobProcessor<T>>();
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped<IJobProcessor<T>, JobProcessor<T>>();
                break;
            case ServiceLifetime.Transient:
                services.AddTransient<IJobProcessor<T>, JobProcessor<T>>();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }


        return services;
    }
}
