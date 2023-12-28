using System;
using System.Collections.Generic;
using System.Linq;

namespace Klab.Toolkit.DI.DependencyFactory;

/// <summary>
/// Generic Factory to resolve single implementation or multiple implementations of the same interface with a defined key
/// </summary>
/// <typeparam name="TInterface"></typeparam>
public class DependencyFactory<TInterface> : IDependencyFactory<TInterface>
{
    private readonly DependencySpecification<TInterface>[] _factories;

    /// <inheritdoc />
    public IEnumerable<string> Keys => _factories.Select(x => x.Key);

    /// <summary>
    /// Create a new instance of the factory
    ///
    /// It will inject a list of factories to create the instances.
    /// </summary>
    /// <param name="factories"></param>
    public DependencyFactory(IEnumerable<DependencySpecification<TInterface>> factories)
    {
        _factories = factories.ToArray();
    }

    /// <inheritdoc />
    public TInterface GetInstance(string key)
    {
        Func<TInterface>? factory = Array.Find(_factories, x => x.Key == key)?.Factory;
        if (factory is null)
        {
            throw new ArgumentException($"For the type '{key}' does not exists a '{typeof(TInterface).Name}' implementation");
        }

        return factory();
    }

    /// <inheritdoc />
    public IEnumerable<TInterface> GetAllInstances(string key)
    {
        IEnumerable<DependencySpecification<TInterface>> desiredDependencyCallbacks = _factories.Where(x => x.Key == key);
        if (!desiredDependencyCallbacks.Any())
        {
            throw new ArgumentException($"For the type {key} does not exists any '{typeof(TInterface).Name}' implementations");
        }

        return desiredDependencyCallbacks.Select(d => d.Factory());
    }


    /// <inheritdoc />
    public TInterface GetInstanceWithInitializationParameters<TParameter>(string key, TParameter parameter)
    {
        TInterface @interface = GetInstance(key);
        if (@interface is not IInitializable<TParameter> initializeInterface)
        {
            throw new ArgumentException($"The type '{typeof(TInterface).Name}' does not implement '{typeof(IInitializable<TParameter>).Name}'");
        }

        initializeInterface.Initialize(parameter);
        return @interface;
    }
}
