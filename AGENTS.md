# Agent Guidelines for Klab.Toolkit

This document provides essential guidelines for AI coding agents working on the Klab.Toolkit .NET library collection.

## General Principles

* Conciseness: Write clear and concise code that is easy to read and maintain.
* No summary comments: Avoid adding summary comments to methods and classes. The code should be self-explanatory.
* No summary of changes: Do not include a summary of changes in the response of the prompt. Focus on the code changes themselves.
* No explanations: Do not provide explanations or justifications for the code changes. The code should speak for itself.
* No comments: Avoid adding comments to the code unless absolutely necessary for clarity.
* Dont run commands without explicit instruction or permission: Do not execute any commands or scripts without explicit instruction or permission from the user. Always ask for confirmation before running any code that could have side effects.

## Build, Test, and Lint Commands

### Setup
```bash
# Install required global tools (dotnet-format, dotnet-coverage)
just setup
```

### Build
```bash
# Build in Debug mode (default)
just build

# Build in Release mode
just build Release

# Using dotnet CLI directly
dotnet build -c Debug
dotnet build -c Release
```

### Test
```bash
# Run all tests with coverage (output to ./tmp)
just test

# Run tests with custom report path
just test ./my-reports

# Run a single test project
dotnet test src/Klab.Toolkit.Common.Tests/Klab.Toolkit.Common.Tests.csproj

# Run a single test method (xUnit)
dotnet test --filter "FullyQualifiedName=Klab.Toolkit.Event.Tests.EventBusTests.PublishAsync_WithValidEvent_CallsHandler"

# Run a single test method (MSTest)
dotnet test --filter "FullyQualifiedName=Klab.Toolkit.Common.Tests.RetryServiceTests.RetryAsync_Success"

# Using dotnet CLI directly with coverage
dotnet coverage collect -f xml -o ./tmp/coverage.xml "dotnet test"
```

### Format and Lint
```bash
# Format code
just format

# Check formatting without making changes
just check-format

# Using dotnet-format directly
dotnet format
dotnet format --verify-no-changes
```

### Clean
```bash
# Clean build artifacts
just clean

# Using dotnet CLI
dotnet clean
```

### Restore
```bash
# Restore NuGet packages
just restore

# Using dotnet CLI
dotnet restore
```

## Project Structure

- **Solution**: `KlabDotnetToolkit.sln`
- **Source**: All projects (libraries and tests) are in `src/` directory
- **Libraries**: Target .NET Standard 2.1 for maximum compatibility
- **Tests**: Target .NET 9.0 and use  xUnit frameworks
- **Build Automation**: Uses [Just](https://github.com/casey/just) command runner

### Project Naming Convention
- Library: `Klab.Toolkit.[ModuleName]`
- Tests: `Klab.Toolkit.[ModuleName].Tests`
- Namespace matches project name

## Code Style Guidelines

### File Structure
```csharp
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Klab.Toolkit.ModuleName;

/// <summary>
/// XML documentation is required for all public types and members.
/// </summary>
public class MyClass
{
    private readonly ILogger<MyClass> _logger;

    public MyClass(ILogger<MyClass> logger)
    {
        _logger = logger;
    }
}
```

### Key Conventions

1. **Namespaces**: Use file-scoped namespaces (enforced)
   ```csharp
   namespace Klab.Toolkit.ModuleName;  // Correct
   ```

2. **Using Directives**: Place outside namespace (enforced), sort System directives first
   ```csharp
   using System;
   using System.Collections.Generic;
   using Microsoft.Extensions.DependencyInjection;

   namespace Klab.Toolkit.ModuleName;
   ```

3. **Type Inference**: Avoid `var`, use explicit types (warning level)
   ```csharp
   IServiceCollection services = new ServiceCollection();  // Correct
   var services = new ServiceCollection();                 // Avoid
   ```

4. **Naming Conventions**:
   - Private fields: `_camelCase` with underscore prefix (enforced)
   - Public/protected/internal: `PascalCase`
   - Parameters/locals: `camelCase`
   - Interfaces: `IPascalCase` (enforced)
   - Type parameters: `TPascalCase` (enforced)
   - Constants: `PascalCase` regardless of accessibility

5. **Expression-bodied Members**:
   - Properties/accessors: Allowed when single line
   - Methods/constructors: Prefer block body
   ```csharp
   public string Name => _name;                    // Good for properties
   public void DoWork() { /* multiple lines */ }   // Prefer block for methods
   ```

6. **Braces**: Always use braces for control statements (suggestion)
   ```csharp
   if (condition)
   {
       DoSomething();
   }
   ```

7. **Indentation**: 4 spaces for C# files

8. **Line Endings**: CRLF for C# files, LF for shell scripts

9. **Encoding**: UTF-8 with BOM for code files

10. **Modern C# Features**: Use latest C# features
    - File-scoped namespaces ✅
    - Record types for immutable data ✅
    - Pattern matching ✅
    - Nullable reference types ✅ (enabled for libraries, disabled for tests)
    - Top-level statements ✅

### Dependency Injection Pattern

Every library project should expose a `DependencyInjection.cs` file with extension methods:

```csharp
namespace Klab.Toolkit.ModuleName;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    /// <summary>
    /// Adds all Klab.Toolkit.ModuleName services to the service collection.
    /// </summary>
    public static IServiceCollection AddKlabToolkitModuleName(this IServiceCollection services)
    {
        // Register services
        return services;
    }
}
```

### Error Handling

Use the Result pattern from `Klab.Toolkit.Results` for business logic errors instead of exceptions:

```csharp
public Result<User> GetUser(int id)
{
    if (id <= 0)
        return ErrorGeneric.NotFound("User", id.ToString());

    // ... logic
    return Result.Success(user);
}
```

### Documentation

- XML documentation is **required** for all public types and members (enforced)
- Treat documentation warnings as errors
- Each library includes its `Readme.md` in the NuGet package

## Testing Guidelines

1. **Test Frameworks**: Both MSTest and xUnit are used
   - Some projects use MSTest (`[TestClass]`, `[TestMethod]`)
   - Others use xUnit (`[Fact]`, `[Theory]`)

2. **Assertion Libraries**: Use FluentAssertions
   ```csharp
   result.Should().BeTrue();
   value.Should().Be(expected);
   ```

3. **Mocking**: Use NSubstitute
   ```csharp
   var mock = Substitute.For<IMyService>();
   mock.GetData().Returns(expectedData);
   ```

4. **Test Organization**: Mirror library structure in test projects

5. **Coverage**: Tests generate coverage reports in `./tmp/` using coverlet

## Design Philosophy

1. **Modularity**: Each package is independently consumable
2. **Functional Programming**: Leverage Result pattern, immutability, pure functions
3. **Testability**: All abstractions have interfaces for dependency injection
4. **Explicit over Implicit**: Clear error handling, explicit types
5. **Documentation**: Comprehensive XML docs and README files

## CI/CD

- **CI Pipeline**: Runs on push/PR to main (format check, build, test, SonarQube)
- **CD Pipeline**: Triggered by version tags (`V*.*.*`) for NuGet publishing
- **Code Quality**: SonarQube integration for static analysis
- **Warnings as Errors**: All warnings are treated as errors in production code

## Common Package References

All production projects automatically include:
- `Microsoft.Extensions.DependencyInjection.Abstractions` (9.0.2)
- `Microsoft.Extensions.Logging.Abstractions` (9.0.2)
- `Microsoft.CodeAnalysis.NetAnalyzers` (9.0.0)

All test projects automatically include:
- MSTest (2.2.10)
- xUnit (2.8.0)
- FluentAssertions (6.10.0)
- NSubstitute (5.1.0)
- coverlet.collector (3.1.2)

## Important Notes

- **No public fields**: Use properties instead (enforced for non-static)
- **Nullable contexts**: Enabled for libraries, disabled for tests
- **Package generation**: Automatic on build for library projects
- **Platform target**: AnyCPU for libraries, X64 for tests
- **License**: MIT (included in all packages)
