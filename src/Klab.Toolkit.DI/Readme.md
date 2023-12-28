# KLAB - Depency Injection Toolkit

In this project I try to implement a simple and lightweight dependency injection toolkit. The toolkit contains helper classes for the following tasks:

* Generic Factory to resolve dependencies with a key.
* Register Classes into the factories with a key.

## Generic Factory

In most application it exists always a Factory which the client pass a key and then get the correct implementation of the interface. The toolkit contains a generic factory which can be used for this purpose. For the Factory it can be use with some extension methods to register the classes into the factory. The extension methods try always register the factory for the specific interface and if the factory already exists then the class will be added to the factory. The factory can be used as follows:

```csharp

// classes
public interface IDependency
{
    string Name { get; }
}

public class Dependency1 : IDependency
{
    public string Name => "one";
}

public class Dependency2 : IDependency
{
    public string Name => "two";
}

// Register interfae and a implemetation to the Microsoft built in DI container
services.AddFactoryMethodTransient<IDependency, Dependency1>("one");
services.AddFactoryMethodTransient<IDependency, Dependency2>("two");
service_builder = services.BuildServiceProvider();

IDependencyFactory<IDependeny> factory = service_builder.GetService<IDependencyFactory<IDependeny>>();
IDependency one = factory.GetInstance("one");
IDependency two = factory.GetInstance("two");
IEnumerable<IDependency> all = factory.GetAllInstances();
IEnumerable<string> keys = factory.Keys;

```

## Generic Factory - Initialize with a common parameter interface
