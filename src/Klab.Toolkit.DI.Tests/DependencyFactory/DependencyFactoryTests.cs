using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;

namespace Klab.Toolkit.DI.DependencyFactory.Tests;
[TestClass()]
public class DependencyFactoryTests
{
    private DependencyFactory<IDependency> _sut;

    [TestInitialize]
    public void Setup()
    {
        _sut = new DependencyFactory<IDependency>(new List<DependencySpecification<IDependency>>
        {
            DependencySpecification<IDependency>.Create("1", () => new Dependency1()),
            DependencySpecification<IDependency>.Create("2", () => new Dependency2()),
            DependencySpecification<IDependency>.Create("3", () => new Dependency3())
        });
    }

    [TestMethod()]
    public void GetInstanceTest()
    {
        IDependency dependency = _sut.GetInstance("1");
        Assert.IsInstanceOfType(dependency, typeof(Dependency1));
    }

    [TestMethod()]
    public void GetAllInstancesTest()
    {
        IEnumerable<IDependency> dependencies = _sut.GetAllInstances("1");
        Assert.AreEqual(1, dependencies.Count());
        Assert.IsInstanceOfType(dependencies.First(), typeof(Dependency1));
    }

    [TestMethod()]
    public void GetAllInstancesTest2()
    {
        IEnumerable<IDependency> dependencies = _sut.GetAllInstances("2");
        Assert.AreEqual(1, dependencies.Count());
        Assert.IsInstanceOfType(dependencies.First(), typeof(Dependency2));
    }

    [TestMethod()]
    public void GetAllInstancesTest3()
    {
        _sut.Invoking(x => x.GetAllInstances("4"))
            .Should()
            .Throw<ArgumentException>();
    }

    [TestMethod()]
    public void Keys_Should_Return_All_Keys()
    {
        IEnumerable<string> keys = _sut.Keys;
        Assert.AreEqual(3, keys.Count());
        Assert.IsTrue(keys.Contains("1"));
        Assert.IsTrue(keys.Contains("2"));
        Assert.IsTrue(keys.Contains("3"));
    }

    [TestMethod()]
    public void GetInstanceWithInitializationParametersTest()
    {
        IDependency dependency1 = _sut.GetInstanceWithInitializationParameters("1", new Dependencyparameters() { Id = 1 });
        IDependency dependency2 = _sut.GetInstanceWithInitializationParameters("2", new Dependencyparameters() { Id = 1 });

        dependency1.Should().BeOfType<Dependency1>();
        dependency1.Id.Should().Be(1);
        dependency2.Should().BeOfType<Dependency2>();
        dependency2.Id.Should().Be(2);
    }

    [TestMethod()]
    public void GetInstanceWithInitializationParametersTest_Should_Fail_If_Interface_Does_Not_Implement_IInitializable()
    {
        _sut.Invoking(x => x.GetInstanceWithInitializationParameters("3", new Dependencyparameters() { Id = 1 }))
            .Should()
            .Throw<ArgumentException>();
    }
}


internal interface IDependency
{
    int Id { get; }
}

internal sealed class Dependency1 : IDependency, IInitializable<Dependencyparameters>
{
    public int Id { get; private set; }

    public void Initialize(Dependencyparameters parameter)
    {
        Id = parameter.Id;
    }
}

internal sealed class Dependency2 : IDependency, IInitializable<Dependencyparameters>
{
    public int Id { get; private set; }

    public void Initialize(Dependencyparameters parameter)
    {
        Id = parameter.Id + 1;
    }
}

internal sealed class Dependency3 : IDependency
{
    public int Id { get; private set; }
}

internal sealed record Dependencyparameters
{
    public int Id { get; set; }
}
