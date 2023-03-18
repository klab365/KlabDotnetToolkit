using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.DI;

/// <summary>
/// Interface for a module.
///
/// A module is a collection of services that can be registered in the DI container. The idea is to
/// have a module per feature, so that the services are registered in the container only when the features is required.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Register module services in the DI container.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    IServiceCollection RegisterModule(IServiceCollection builder);
}
