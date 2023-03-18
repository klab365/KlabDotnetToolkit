using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.DI;

/// <summary>
/// Extension for the Module interface.
/// </summary>
public static class ModuleExtensions
{
    /// <summary>
    /// Register all the modules in the current assembly.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterModules(this IServiceCollection services)
    {
        IEnumerable<IModule> modules = DiscoverModules();
        foreach (IModule module in modules)
        {
            services = module.RegisterModule(services);
        }

        return services;
    }

    /// <summary>
    /// Discover all the modules in the current assembly.
    /// </summary>
    /// <returns></returns>
    private static IEnumerable<IModule> DiscoverModules()
    {
        return typeof(IModule).Assembly
            .GetTypes()
            .Where(t => t.IsClass && t.IsAssignableTo(typeof(IModule)))
            .Select(Activator.CreateInstance)
            .Cast<IModule>();
    }
}
