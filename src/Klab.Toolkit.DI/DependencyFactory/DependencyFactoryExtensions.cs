using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Klab.Toolkit.DI.DependencyFactory;

/// <summary>
/// Extension to add the factory to the dependency injection and register the implementations
/// </summary>
public static class DependencyFactoryExtensions
{
    /// <summary>
    /// Register interface and implementation in the dependency injection with the factory pattern.
    /// The <typeparamref name="TInstanceType"/> will be registered as transient.
    ///
    /// The factory pattern looks like follow:
    /// * register interface with the concrete implementation
    /// * register (TKey, Func<typeparamref name="TInterface"/>) to create the instance of the implementation
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TInstanceType"></typeparam>
    /// <param name="services"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static IServiceCollection AddFactoryMethodTransient<TInterface, TInstanceType>(this IServiceCollection services, string key) where TInstanceType : class, TInterface
    {
        services.AddTransient<TInstanceType>();
        services.AddDependencySpecificationMethod<TInterface, TInstanceType>(key);
        return services;
    }

    /// <summary>
    /// Register interface and implementation in the dependency injection with the factory pattern.
    /// The <typeparamref name="TInstanceType"/> will be registered as singelton.
    ///
    /// The factory pattern looks like follow:
    /// * register interface with the concrete implementation
    /// * register (TKey, Func<typeparamref name="TInterface"/>) to create the instance of the implementation
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TInstanceType"></typeparam>
    /// <param name="services"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static IServiceCollection AddFactoryMethodSingelton<TInterface, TInstanceType>(this IServiceCollection services, string key) where TInstanceType : class, TInterface
    {
        services.AddSingleton<TInstanceType>();
        services.AddDependencySpecificationMethod<TInterface, TInstanceType>(key);
        return services;
    }

    /// <summary>
    /// Register interface and implementation in the dependency injection with the factory pattern.
    /// The <typeparamref name="TInstanceType"/> will be registered as scoped.
    ///
    /// The factory pattern looks like follow:
    /// * register interface with the concrete implementation
    /// * register (TKey, Func<typeparamref name="TInterface"/>) to create the instance of the implementation
    /// </summary>
    /// <param name="services"></param>
    /// <param name="key"></param>
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TInstanceType"></typeparam>
    /// <returns></returns>
    public static IServiceCollection AddFactoryMethodScoped<TInterface, TInstanceType>(this IServiceCollection services, string key) where TInstanceType : class, TInterface
    {
        services.AddScoped<TInstanceType>();
        services.AddDependencySpecificationMethod<TInterface, TInstanceType>(key);
        return services;
    }

    private static IServiceCollection AddDependencySpecificationMethod<TInterface, TInstanceType>(this IServiceCollection services, string key) where TInstanceType : class, TInterface
    {
        services.AddTransient((provider) =>
        {
            DependencySpecification<TInterface> dependencySpecification = DependencySpecification<TInterface>.Create(key, () => provider.GetRequiredService<TInstanceType>());
            return dependencySpecification;
        });

        // factory itself, if not registered
        services.TryAddTransient<IDependencyFactory<TInterface>, DependencyFactory<TInterface>>();
        return services;
    }
}
