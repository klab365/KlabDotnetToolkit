using System.Collections.Generic;

namespace Klab.Toolkit.DI.DependencyFactory;

/// <summary>
/// Interface for a generic dependency factory.
/// </summary>
/// <typeparam name="TInterface"></typeparam>
public interface IDependencyFactory<TInterface>
{
    /// <summary>
    /// Keys registered in the factory
    /// </summary>
    IEnumerable<string> Keys { get; }

    /// <summary>
    /// Get all the instances of the implementations for the given key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IEnumerable<TInterface> GetAllInstances(string key);

    /// <summary>
    /// Get the instance of the implementation for the given key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    TInterface GetInstance(string key);

    /// <summary>
    /// Get the instance of the implementation for the given key and initialize it with the given parameter
    /// </summary>
    /// <param name="key"></param>
    /// <param name="parameter"></param>
    /// <typeparam name="TParameter"></typeparam>
    /// <returns></returns>
    TInterface GetInstanceWithInitializationParameters<TParameter>(string key, TParameter parameter);
}
