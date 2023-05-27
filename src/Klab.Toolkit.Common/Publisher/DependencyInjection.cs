using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klab.Toolkit.Common.Publisher;

/// <summary>
/// Dependency injection for publisher
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add publisher to the dependency injection.
    /// For every Type of data it exist a publisher proxy (singelton) and a publisher (transient).
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddPublisher<T>(this IServiceCollection services)
    {
        services.TryAddSingleton<IPublisherProxy<T>, PublisherProxy<T>>();
        services.TryAddTransient<IPublisher<T>, Publisher<T>>();
        return services;
    }
}
