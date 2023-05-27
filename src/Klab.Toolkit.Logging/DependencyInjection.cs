using Klab.Toolkit.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Klab.Toolkit.Logging;

/// <summary>
/// Extension for the dependency injection to add logging infrastructure.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add logger publisher to the dependency injection.
    /// </summary>
    /// <param name="loggingBuilder"></param>
    /// <returns></returns>
    public static ILoggingBuilder AddLoggerPublisher(this ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.Services.TryAddTransient<ITimeProvider, TimeProvider>();
        loggingBuilder.Services.TryAddSingleton<ILoggerPublisherProxy, LoggerPublisherProxy>();
        loggingBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, LoggerPublisherProvider>());
        return loggingBuilder;
    }
}
