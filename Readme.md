# Klab.Toolkit

A comprehensive collection of .NET libraries designed to accelerate development with proven patterns, utilities, and abstractions. Each package can be consumed independently, providing maximum flexibility for your .NET applications.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-netstandard2.1-blue.svg)](https://dotnet.microsoft.com/)

## 🚀 Quick Start

Install individual packages as needed:

```bash
# Event-driven architecture
dotnet add package Klab.Toolkit.Event

# Functional error handling
dotnet add package Klab.Toolkit.Results

# Common utilities and abstractions
dotnet add package Klab.Toolkit.Common

# Extension methods for common types
dotnet add package Klab.Toolkit.ExtensionMethods

# Dependency injection utilities
dotnet add package Klab.Toolkit.DI

# Value objects and domain primitives
dotnet add package Klab.Toolkit.ValueObjects
```

## 📦 Packages

### 🎯 [Klab.Toolkit.Event](src/Klab.Toolkit.Event/)
**Event-driven architecture made simple**

A comprehensive event-driven communication system supporting three patterns:
- **Event Publishing/Subscribing**: Fire-and-forget notifications with multiple handlers
- **Request/Response**: Commands and queries with guaranteed single responses (MediatR-like)
- **Stream Request/Response**: Handle requests returning multiple values over time

```csharp
// Event publishing
await eventBus.PublishAsync(new UserRegisteredEvent(userId, email));

// Request/response
var user = await eventBus.SendAsync(new GetUserQuery(userId));

// Streaming responses
await foreach (var activity in eventBus.Stream(new GetUserActivityStream(userId)))
{
    // Process real-time data
}
```

**Key Features:**
- Pluggable message queues (in-memory, Redis, etc.)
- Built-in error handling with Result pattern
- Event logging and sensitive data protection
- Background processing with hosted services

---

### ✅ [Klab.Toolkit.Results](src/Klab.Toolkit.Results/)
**Functional error handling without exceptions**

Implements the Result pattern for explicit error handling, eliminating unexpected exceptions and making error flows predictable.

```csharp
// Chain operations that might fail
Result<ProcessedData> result = Result.Success(inputData)
    .Bind(data => ValidateData(data))
    .Map(data => TransformData(data))
    .Bind(data => SaveData(data))
    .OnSuccess(data => LogSuccess(data))
    .OnFailure(error => LogError(error));

// Handle both cases explicitly
string message = result.Match(
    onSuccess: data => $"Processed {data.Count} items",
    onFailure: error => $"Failed: {error.Message}"
);
```

**Extension Methods:**
- `Map`: Transform success values
- `Bind`: Chain operations that can fail
- `OnSuccess`/`OnFailure`: Side effects without changing the result
- `Match`: Handle both success and failure cases

---

### 🔧 [Klab.Toolkit.Common](src/Klab.Toolkit.Common/)
**Shared abstractions and utilities**

Common interfaces and utilities used across the toolkit, promoting consistency and testability.

```csharp
// Testable time operations
public class OrderService
{
    private readonly ITimeProvider _timeProvider;

    public Order CreateOrder()
    {
        return new Order
        {
            CreatedAt = _timeProvider.UtcNow,
            ExpiresAt = _timeProvider.UtcNow.AddDays(30)
        };
    }
}
```

**Key Components:**
- `ITimeProvider`: Abstraction for system time (testing-friendly)
- `ITaskProvider`: Task creation abstraction
- `IRetryService`: Configurable retry policies
- `JobProcessor`: Background job processing utilities

---

### 🚀 [Klab.Toolkit.ExtensionMethods](src/Klab.Toolkit.ExtensionMethods/)
**Powerful extensions for common types**

Extension methods that enhance built-in .NET types with frequently needed functionality.

```csharp
// String extensions
string text = "hello world";
string pascal = text.ToPascalCase(); // "HelloWorld"
bool isEmpty = text.IsNullOrWhiteSpace();

// Enum extensions
MyEnum value = MyEnum.SomeValue;
string description = value.GetDescription(); // Gets DescriptionAttribute value
T parsed = "SomeValue".ParseEnum<MyEnum>();

// Type extensions
bool isNullable = typeof(int?).IsNullable();
Type underlyingType = typeof(int?).GetUnderlyingType();
```

**Extension Categories:**
- **String**: Case conversions, validation, parsing
- **Enum**: Description attributes, parsing, validation
- **Type**: Nullability checks, underlying types

---

### 🏗️ [Klab.Toolkit.DI](src/Klab.Toolkit.DI/)
**Advanced dependency injection patterns**

Utilities for complex dependency injection scenarios, including keyed factories and dynamic resolution.

```csharp
// Generic keyed factory
services.AddFactory<IPaymentProcessor, string>()
    .AddImplementation<PayPalProcessor>("paypal")
    .AddImplementation<StripeProcessor>("stripe")
    .AddImplementation<SquareProcessor>("square");

// Usage
var factory = serviceProvider.GetRequiredService<IFactory<IPaymentProcessor, string>>();
var processor = factory.Create("stripe");
```

**Features:**
- **Keyed Factories**: Resolve implementations by string keys
- **Dynamic Registration**: Add implementations to factories at runtime
- **Type-Safe**: Compile-time type checking for factory implementations

---

### 💎 [Klab.Toolkit.ValueObjects](src/Klab.Toolkit.ValueObjects/)
**Domain-driven design value objects**

Base classes and utilities for creating immutable value objects that encapsulate business rules and validation.

```csharp
// Custom value object
public sealed class EmailAddress : ValueObject
{
    public string Value { get; }

    private EmailAddress(string value)
    {
        Value = value;
    }

    public static Result<EmailAddress> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Error.Create("INVALID_EMAIL", "Email cannot be empty");

        if (!IsValidFormat(email))
            return Error.Create("INVALID_EMAIL", "Email format is invalid");

        return new EmailAddress(email);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }
}
```

**Benefits:**
- **Immutability**: Value objects cannot be changed after creation
- **Validation**: Business rules enforced at creation time
- **Equality**: Proper value-based equality semantics

## 🛠️ Development

### Prerequisites

- [.NET SDK 6.0+](https://dotnet.microsoft.com/download)
- [Just](https://github.com/casey/just) - Command runner (optional but recommended)

### Building the Solution

```bash
# Using Just (recommended)
just build

# Or using dotnet CLI
dotnet build
```

### Running Tests

```bash
# All tests
just test

# Or using dotnet CLI
dotnet test
```

### Available Commands

```bash
# Clean build artifacts
just clean

# Format code
just format

# Add new projects to solution
just add-projects
```

## 📋 Compatibility

- **Target Framework**: .NET Standard 2.1
- **Language**: C# with latest language features
- **Platforms**: Windows, macOS, Linux

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines

- Follow existing code style and patterns
- Add tests for new functionality
- Update documentation for public APIs
- Ensure all tests pass before submitting

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🏷️ Versioning

We use [Semantic Versioning](https://semver.org/). For available versions, see the [tags on this repository](https://github.com/klab365/KlabDotnetToolkit/tags).

See [CHANGELOG.md](Changelog.md) for detailed version history.

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/klab365/KlabDotnetToolkit/issues)
- **Discussions**: [GitHub Discussions](https://github.com/klab365/KlabDotnetToolkit/discussions)

## 🎯 Design Philosophy

This toolkit follows these core principles:

- **Modularity**: Each package solves specific problems and can be used independently
- **Functional Programming**: Emphasis on immutability, pure functions, and explicit error handling
- **Testability**: All components are designed with testing in mind
- **Performance**: Optimized for high-performance scenarios with minimal allocations
- **Developer Experience**: Clear APIs, comprehensive documentation, and helpful error messages

---

*Built with ❤️ by the Klab team*
