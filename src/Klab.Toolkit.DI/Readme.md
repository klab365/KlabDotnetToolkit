# Klab.Toolkit.DI

## Overview

The `Klab.Toolkit.DI` package provides advanced dependency injection patterns and utilities that extend the capabilities of Microsoft's built-in DI container. It specializes in solving complex scenarios like keyed service resolution, dynamic factory patterns, and runtime service discovery.

## Purpose

This lightweight dependency injection toolkit addresses common enterprise patterns:

- **Keyed Service Resolution**: Resolve different implementations based on string keys
- **Dynamic Factories**: Create services with runtime parameters and initialization
- **Strategy Pattern Implementation**: Easy registration and resolution of strategy implementations
- **Plugin Architecture**: Support for modular application design

## Key Features

### Core Components

- **`IDependencyFactory<T>`**: Generic factory interface for keyed service resolution
- **Factory Extensions**: Fluent registration methods for Microsoft.Extensions.DependencyInjection
- **Initialization Support**: Services can receive parameters during creation
- **Lifecycle Management**: Support for all standard DI lifetimes (Transient, Scoped, Singleton)

## Installation

```bash
dotnet add package Klab.Toolkit.DI
```

## Core Interface

```csharp
public interface IDependencyFactory<TInterface>
{
    IEnumerable<string> Keys { get; }
    TInterface GetInstance(string key);
    IEnumerable<TInterface> GetAllInstances(string key);
    TInterface GetInstanceWithInitializationParameters<TParameter>(string key, TParameter parameter);
}
```

## Basic Usage

### Simple Factory Registration

```csharp
// Define your interface and implementations
public interface IPaymentProcessor
{
    Task<PaymentResult> ProcessAsync(Payment payment);
    string ProviderName { get; }
}

public class PayPalProcessor : IPaymentProcessor
{
    public string ProviderName => "PayPal";

    public async Task<PaymentResult> ProcessAsync(Payment payment)
    {
        // PayPal-specific implementation
        await Task.Delay(100);
        return new PaymentResult { Success = true, TransactionId = Guid.NewGuid().ToString() };
    }
}

public class StripeProcessor : IPaymentProcessor
{
    public string ProviderName => "Stripe";

    public async Task<PaymentResult> ProcessAsync(Payment payment)
    {
        // Stripe-specific implementation
        await Task.Delay(150);
        return new PaymentResult { Success = true, TransactionId = Guid.NewGuid().ToString() };
    }
}

// Registration in Startup.cs or Program.cs
services.AddFactoryMethodTransient<IPaymentProcessor, PayPalProcessor>("paypal");
services.AddFactoryMethodTransient<IPaymentProcessor, StripeProcessor>("stripe");
services.AddFactoryMethodTransient<IPaymentProcessor, SquareProcessor>("square");

// Usage in your services
public class PaymentService
{
    private readonly IDependencyFactory<IPaymentProcessor> _paymentFactory;

    public PaymentService(IDependencyFactory<IPaymentProcessor> paymentFactory)
    {
        _paymentFactory = paymentFactory;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(Payment payment, string provider)
    {
        var processor = _paymentFactory.GetInstance(provider);
        return await processor.ProcessAsync(payment);
    }

    public IEnumerable<string> GetAvailableProviders()
    {
        return _paymentFactory.Keys;
    }
}
```

### Advanced Factory Patterns

#### Multiple Implementations per Key

```csharp
// Register multiple implementations for the same key
services.AddFactoryMethodTransient<INotificationService, EmailNotificationService>("email");
services.AddFactoryMethodTransient<INotificationService, SmsNotificationService>("sms");
services.AddFactoryMethodTransient<INotificationService, PushNotificationService>("push");

// Also register a composite service
services.AddFactoryMethodTransient<INotificationService, CompositeNotificationService>("all");

public class NotificationService
{
    private readonly IDependencyFactory<INotificationService> _notificationFactory;

    public async Task SendNotificationAsync(string message, string[] channels)
    {
        foreach (var channel in channels)
        {
            var services = _notificationFactory.GetAllInstances(channel);
            foreach (var service in services)
            {
                await service.SendAsync(message);
            }
        }
    }

    public async Task SendToAllChannelsAsync(string message)
    {
        var compositeService = _notificationFactory.GetInstance("all");
        await compositeService.SendAsync(message);
    }
}
```

#### Service Initialization with Parameters

```csharp
public interface IReportGenerator
{
    Task<byte[]> GenerateAsync();
}

public class PdfReportGenerator : IReportGenerator, IInitialize<ReportConfig>
{
    private ReportConfig _config;

    public void Initialize(ReportConfig parameter)
    {
        _config = parameter ?? throw new ArgumentNullException(nameof(parameter));
    }

    public async Task<byte[]> GenerateAsync()
    {
        // Generate PDF using _config
        await Task.Delay(100);
        return new byte[] { /* PDF data */ };
    }
}

public class ExcelReportGenerator : IReportGenerator, IInitialize<ReportConfig>
{
    private ReportConfig _config;

    public void Initialize(ReportConfig parameter)
    {
        _config = parameter;
    }

    public async Task<byte[]> GenerateAsync()
    {
        // Generate Excel using _config
        await Task.Delay(80);
        return new byte[] { /* Excel data */ };
    }
}

// Registration
services.AddFactoryMethodTransient<IReportGenerator, PdfReportGenerator>("pdf");
services.AddFactoryMethodTransient<IReportGenerator, ExcelReportGenerator>("excel");

// Usage with initialization
public class ReportService
{
    private readonly IDependencyFactory<IReportGenerator> _reportFactory;

    public async Task<byte[]> GenerateReportAsync(string format, ReportConfig config)
    {
        var generator = _reportFactory.GetInstanceWithInitializationParameters(format, config);
        return await generator.GenerateAsync();
    }
}

public record ReportConfig(string Title, DateTime StartDate, DateTime EndDate, string[] Columns);
```

## Real-World Scenarios

### 1. Multi-Tenant Data Access

```csharp
public interface ITenantRepository
{
    Task<IEnumerable<T>> GetDataAsync<T>(string query);
}

public class SqlServerTenantRepository : ITenantRepository, IInitialize<ConnectionConfig>
{
    private ConnectionConfig _config;

    public void Initialize(ConnectionConfig config)
    {
        _config = config;
    }

    public async Task<IEnumerable<T>> GetDataAsync<T>(string query)
    {
        // Implementation using SQL Server with _config.ConnectionString
        await Task.Delay(50);
        return Enumerable.Empty<T>();
    }
}

public class CosmosDbTenantRepository : ITenantRepository, IInitialize<ConnectionConfig>
{
    private ConnectionConfig _config;

    public void Initialize(ConnectionConfig config)
    {
        _config = config;
    }

    public async Task<IEnumerable<T>> GetDataAsync<T>(string query)
    {
        // Implementation using Cosmos DB
        await Task.Delay(30);
        return Enumerable.Empty<T>();
    }
}

// Registration based on tenant configuration
services.AddFactoryMethodScoped<ITenantRepository, SqlServerTenantRepository>("sqlserver");
services.AddFactoryMethodScoped<ITenantRepository, CosmosDbTenantRepository>("cosmosdb");

public class TenantService
{
    private readonly IDependencyFactory<ITenantRepository> _repositoryFactory;

    public async Task<IEnumerable<Customer>> GetCustomersAsync(string tenantId)
    {
        var tenantConfig = await GetTenantConfigAsync(tenantId);
        var repository = _repositoryFactory.GetInstanceWithInitializationParameters(
            tenantConfig.DatabaseType,
            tenantConfig.ConnectionConfig
        );

        return await repository.GetDataAsync<Customer>("SELECT * FROM Customers");
    }
}
```

### 2. Plugin-Based Architecture

```csharp
public interface IDataProcessor
{
    string SupportedFormat { get; }
    Task<ProcessingResult> ProcessAsync(Stream data);
}

public class JsonDataProcessor : IDataProcessor
{
    public string SupportedFormat => "json";

    public async Task<ProcessingResult> ProcessAsync(Stream data)
    {
        // Process JSON data
        await Task.Delay(100);
        return new ProcessingResult { Success = true, RecordsProcessed = 100 };
    }
}

public class XmlDataProcessor : IDataProcessor
{
    public string SupportedFormat => "xml";

    public async Task<ProcessingResult> ProcessAsync(Stream data)
    {
        // Process XML data
        await Task.Delay(120);
        return new ProcessingResult { Success = true, RecordsProcessed = 85 };
    }
}

public class CsvDataProcessor : IDataProcessor
{
    public string SupportedFormat => "csv";

    public async Task<ProcessingResult> ProcessAsync(Stream data)
    {
        // Process CSV data
        await Task.Delay(80);
        return new ProcessingResult { Success = true, RecordsProcessed = 200 };
    }
}

// Auto-registration of all processors
public static class ProcessorRegistration
{
    public static void RegisterDataProcessors(this IServiceCollection services)
    {
        var processorTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IDataProcessor).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var processorType in processorTypes)
        {
            var instance = (IDataProcessor)Activator.CreateInstance(processorType);
            services.AddFactoryMethodTransient<IDataProcessor>(instance.SupportedFormat, processorType);
        }
    }
}

// Usage
public class FileProcessingService
{
    private readonly IDependencyFactory<IDataProcessor> _processorFactory;

    public async Task<ProcessingResult> ProcessFileAsync(string filePath)
    {
        var extension = Path.GetExtension(filePath).TrimStart('.').ToLower();

        try
        {
            var processor = _processorFactory.GetInstance(extension);
            using var fileStream = File.OpenRead(filePath);
            return await processor.ProcessAsync(fileStream);
        }
        catch (KeyNotFoundException)
        {
            throw new UnsupportedFormatException($"No processor found for format: {extension}");
        }
    }

    public IEnumerable<string> GetSupportedFormats()
    {
        return _processorFactory.Keys;
    }
}
```

### 3. Configuration-Driven Service Selection

```csharp
public interface IEmailService
{
    Task SendAsync(EmailMessage message);
}

public class SmtpEmailService : IEmailService, IInitialize<SmtpConfig>
{
    private SmtpConfig _config;

    public void Initialize(SmtpConfig config)
    {
        _config = config;
    }

    public async Task SendAsync(EmailMessage message)
    {
        // SMTP implementation
        await Task.Delay(200);
    }
}

public class SendGridEmailService : IEmailService, IInitialize<SendGridConfig>
{
    private SendGridConfig _config;

    public void Initialize(SendGridConfig config)
    {
        _config = config;
    }

    public async Task SendAsync(EmailMessage message)
    {
        // SendGrid API implementation
        await Task.Delay(100);
    }
}

// Configuration-based registration
public class EmailServiceConfiguration
{
    public static void ConfigureEmailServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddFactoryMethodTransient<IEmailService, SmtpEmailService>("smtp");
        services.AddFactoryMethodTransient<IEmailService, SendGridEmailService>("sendgrid");
        services.AddFactoryMethodTransient<IEmailService, AmazonSesEmailService>("ses");

        // Register a configuration-aware service
        services.AddSingleton<IEmailServiceResolver, EmailServiceResolver>();
    }
}

public class EmailServiceResolver
{
    private readonly IDependencyFactory<IEmailService> _emailFactory;
    private readonly IConfiguration _configuration;

    public EmailServiceResolver(IDependencyFactory<IEmailService> emailFactory, IConfiguration configuration)
    {
        _emailFactory = emailFactory;
        _configuration = configuration;
    }

    public IEmailService GetEmailService(string environment = null)
    {
        environment ??= _configuration["Environment"];

        var provider = environment.ToLower() switch
        {
            "development" => "smtp",
            "staging" => "sendgrid",
            "production" => "ses",
            _ => throw new InvalidOperationException($"No email provider configured for environment: {environment}")
        };

        var config = GetConfigForProvider(provider);
        return _emailFactory.GetInstanceWithInitializationParameters(provider, config);
    }

    private object GetConfigForProvider(string provider)
    {
        return provider switch
        {
            "smtp" => _configuration.GetSection("Smtp").Get<SmtpConfig>(),
            "sendgrid" => _configuration.GetSection("SendGrid").Get<SendGridConfig>(),
            "ses" => _configuration.GetSection("AmazonSes").Get<AmazonSesConfig>(),
            _ => throw new ArgumentException($"Unknown provider: {provider}")
        };
    }
}
```

## Registration Methods

The package provides several extension methods for registering services:

```csharp
// Transient lifetime
services.AddFactoryMethodTransient<IInterface, Implementation>("key");

// Scoped lifetime
services.AddFactoryMethodScoped<IInterface, Implementation>("key");

// Singleton lifetime
services.AddFactoryMethodSingleton<IInterface, Implementation>("key");

// With specific instance
services.AddFactoryMethodSingleton<IInterface>("key", new Implementation());

// With factory function
services.AddFactoryMethodTransient<IInterface>("key", provider => new Implementation(provider.GetService<IDependency>()));
```

## Best Practices

### 1. Consistent Key Naming
```csharp
// Use lowercase, consistent naming
services.AddFactoryMethodTransient<IProcessor, JsonProcessor>("json");
services.AddFactoryMethodTransient<IProcessor, XmlProcessor>("xml");

// Not: "JSON", "Json", "json", "XML", "Xml"
```

### 2. Fail-Fast Registration
```csharp
public static class ServiceRegistration
{
    public static void RegisterProcessors(this IServiceCollection services)
    {
        // Validate required dependencies are available
        var requiredServices = new[] { typeof(ILogger<>), typeof(IConfiguration) };

        services.AddFactoryMethodTransient<IProcessor, JsonProcessor>("json");
        services.AddFactoryMethodTransient<IProcessor, XmlProcessor>("xml");

        // Validate registration
        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDependencyFactory<IProcessor>>();

        if (!factory.Keys.Any())
            throw new InvalidOperationException("No processors were registered");
    }
}
```

### 3. Graceful Degradation
```csharp
public class RobustService
{
    private readonly IDependencyFactory<IProcessor> _processorFactory;

    public async Task<Result> ProcessDataAsync(string format, object data)
    {
        try
        {
            var processor = _processorFactory.GetInstance(format);
            return await processor.ProcessAsync(data);
        }
        catch (KeyNotFoundException)
        {
            // Fall back to default processor
            var defaultProcessor = _processorFactory.GetInstance("default");
            return await defaultProcessor.ProcessAsync(data);
        }
    }
}
```

### 4. Testing Strategy
```csharp
[Test]
public void ProcessorFactory_ShouldReturnCorrectImplementation()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddFactoryMethodTransient<IProcessor, JsonProcessor>("json");
    services.AddFactoryMethodTransient<IProcessor, XmlProcessor>("xml");

    var provider = services.BuildServiceProvider();
    var factory = provider.GetRequiredService<IDependencyFactory<IProcessor>>();

    // Act
    var jsonProcessor = factory.GetInstance("json");
    var xmlProcessor = factory.GetInstance("xml");

    // Assert
    jsonProcessor.Should().BeOfType<JsonProcessor>();
    xmlProcessor.Should().BeOfType<XmlProcessor>();
    factory.Keys.Should().Contain(new[] { "json", "xml" });
}
```

## Performance Considerations

- **Factory Resolution**: O(1) lookup time using internal dictionaries
- **Memory Overhead**: Minimal - only stores type information and keys
- **Thread Safety**: All factory operations are thread-safe
- **Lifetime Management**: Respects DI container lifetime scopes

## Common Patterns

### Strategy Pattern
Perfect for implementing the Strategy pattern where algorithm selection happens at runtime.

### Plugin Architecture
Enables modular applications where functionality can be extended through plugins.

### Multi-Tenant Applications
Allows tenant-specific service implementations with proper isolation.

### Configuration-Driven Services
Services can be selected based on environment, feature flags, or user preferences.

## Migration from Other Patterns

### From Service Locator Anti-Pattern
```csharp
// Before (Anti-pattern)
public class BadService
{
    public void DoWork()
    {
        var processor = ServiceLocator.GetService<IProcessor>("json");
        // ... work
    }
}

// After (Dependency Injection)
public class GoodService
{
    private readonly IDependencyFactory<IProcessor> _processorFactory;

    public GoodService(IDependencyFactory<IProcessor> processorFactory)
    {
        _processorFactory = processorFactory;
    }

    public void DoWork(string format)
    {
        var processor = _processorFactory.GetInstance(format);
        // ... work
    }
}
```

### From Manual Factory Pattern
```csharp
// Before (Manual factory)
public class ManualProcessorFactory
{
    public IProcessor CreateProcessor(string type)
    {
        return type switch
        {
            "json" => new JsonProcessor(),
            "xml" => new XmlProcessor(),
            _ => throw new ArgumentException($"Unknown type: {type}")
        };
    }
}

// After (DI Factory)
// Just register and inject IDependencyFactory<IProcessor>
```
