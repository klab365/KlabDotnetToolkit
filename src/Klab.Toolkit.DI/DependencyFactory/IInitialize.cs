namespace Klab.Toolkit.DI.DependencyFactory;

/// <summary>
/// Common interface for a class which needs parameterless initialization after creation.
/// </summary>
public interface IInitializable
{
    /// <summary>
    /// Initialize the instance.
    /// </summary>
    void Initialize();
}

/// <summary>
/// Common interface for a class which needs initialization with a parameter after creation.
/// </summary>
/// <typeparam name="T">The type of the parameter.</typeparam>
public interface IInitializable<in T>
{
    /// <summary>
    /// Initialize the instance with the given parameter.
    /// </summary>
    /// <param name="parameter"></param>
    void Initialize(T parameter);
}
