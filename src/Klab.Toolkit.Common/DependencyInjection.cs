using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.Common;

/// <summary>
/// Dependency injection extensions for the Klab.Toolkit.Common library.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add all services to the service collection.
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AddAll(IServiceCollection services)
    {
        services.AddRetryService();
        services.AddTimeProvider();
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
}
