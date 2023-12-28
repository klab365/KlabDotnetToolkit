using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.DI.DependencyFactory.Tests;

[TestClass()]
public class DependencyFactoryExtensionsTests
{
    [TestMethod()]
    public void AddFactoryTransientTest()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddFactoryMethodTransient<IDependency, Dependency1>("1");
        services.AddFactoryMethodTransient<IDependency, Dependency2>("2");
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        IDependencyFactory<IDependency> dependencyFactory = serviceProvider.GetRequiredService<IDependencyFactory<IDependency>>();
        IDependency dependency1 = dependencyFactory.GetInstance("1");
        IDependency dependency1Again = dependencyFactory.GetInstance("1");

        dependencyFactory.Keys.Count().Should().Be(2);
        dependency1.Should().NotBeSameAs(dependency1Again);
    }

    [TestMethod()]
    public void AddFactorySingeltonTest()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddFactoryMethodSingelton<IDependency, Dependency1>("1");
        services.AddFactoryMethodSingelton<IDependency, Dependency2>("2");
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        IDependencyFactory<IDependency> dependencyFactory = serviceProvider.GetRequiredService<IDependencyFactory<IDependency>>();
        IDependency dependency1 = dependencyFactory.GetInstance("1");
        IDependency dependency1Again = dependencyFactory.GetInstance("1");

        dependencyFactory.Keys.Count().Should().Be(2);
        dependency1.Should().BeSameAs(dependency1Again);
    }

    [TestMethod]
    public void AddFactoryScopedTest()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddFactoryMethodScoped<IDependency, Dependency1>("1");
        services.AddFactoryMethodScoped<IDependency, Dependency2>("2");
        ServiceProvider serviceProvider = services.BuildServiceProvider();

        IDependencyFactory<IDependency> dependencyFactory = serviceProvider.GetRequiredService<IDependencyFactory<IDependency>>();
        IDependency dependency1 = dependencyFactory.GetInstance("1");
        IDependency dependency1Again = dependencyFactory.GetInstance("1");

        dependencyFactory.Keys.Count().Should().Be(2);
        dependency1.Should().BeSameAs(dependency1Again);
    }
}
