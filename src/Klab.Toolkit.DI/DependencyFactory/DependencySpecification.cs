using System;

namespace Klab.Toolkit.DI.DependencyFactory;

/// <summary>
/// Specification to register the factory method in the dependency injection
/// </summary>
/// <typeparam name="TInterface"></typeparam>
/// <returns></returns>
public record DependencySpecification<TInterface>
{
    /// <summary>
    /// Key to register the factory
    /// </summary>
    /// <value></value>
    public string Key { get; private set; } = string.Empty;

    /// <summary>
    /// Factory method to create the instance
    /// </summary>
    /// <value></value>
    public Func<TInterface> Factory { get; private set; }

    /// <summary>
    /// Create a new valid specification
    ///
    /// </summary>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    /// <returns></returns>
    public static DependencySpecification<TInterface> Create(string key, Func<TInterface> factory)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("If string is empty, then it doesn't make senso to register as factory", nameof(key));
        }

        return new(key, factory);
    }

    /// <summary>
    /// Private constructor to force the use of the factory method
    /// </summary>
    /// <param name="key"></param>
    /// <param name="factory"></param>
    private DependencySpecification(string key, Func<TInterface> factory)
    {
        Key = key;
        Factory = factory;
    }
}
