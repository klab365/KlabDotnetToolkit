# Klab.Toolkit.Common

## Overview

The `Klab.Toolkit.Common` package provides essential abstractions, utilities, and services that promote testability, consistency, and reusability across .NET applications. This package serves as the foundation for building maintainable applications by offering well-designed interfaces and implementations for common cross-cutting concerns.

## Purpose

The primary purpose of the `Klab.Toolkit.Common` package is to:

- **Promote Testability**: Abstract system dependencies like time, tasks, and I/O operations
- **Ensure Consistency**: Provide standardized interfaces for common operations
- **Enable Reusability**: Offer utility classes that solve frequent development challenges
- **Support Modularity**: Define clear contracts between application components

## Key Features

### Core Abstractions

- **`ITimeProvider`**: Abstracts system time operations for testable time-dependent code
- **`ITaskProvider`**: Provides thread-safe task and locking operations
- **`IRetryService`**: Implements configurable retry policies with exponential backoff
- **`JobProcessor<T>`**: Generic background job processing with channels

### Utility Classes

- **Time Operations**: Testable alternatives to `DateTime.Now` and `Task.Delay`
- **Retry Logic**: Robust error handling with configurable retry strategies
- **Background Processing**: High-performance job queuing and processing
- **Resource Locking**: Thread-safe resource access with proper cleanup

## Installation

```bash
dotnet add package Klab.Toolkit.Common
```

## Setup

Register all services in your dependency injection container:

```csharp
using Klab.Toolkit.Common;

// In Program.cs or Startup.cs
services.AddKlabToolkitCommon();
```

This registers:
- `ITimeProvider` → `TimeProvider`
- `ITaskProvider` → `TaskProvider`
- `IRetryService` → `RetryService`

## Core Components

### ITimeProvider - Testable Time Operations

The `ITimeProvider` interface abstracts system time, making your code testable and time-zone aware.

#### Interface

```csharp
public interface ITimeProvider
{
    DateTimeOffset GetCurrentTime();
    Task<Result> WaitAsync(TimeSpan timeSpan, CancellationToken cancellationToken = default);
}
```

#### Usage Examples

```csharp
public class OrderService
{
    private readonly ITimeProvider _timeProvider;

    public OrderService(ITimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        var now = _timeProvider.GetCurrentTime();
        
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CreatedAt = now,
            ExpiresAt = now.AddDays(30),
            Items = request.Items
        };

        // Wait before processing (respects cancellation)
        await _timeProvider.WaitAsync(TimeSpan.FromSeconds(1));
        
        return order;
    }

    public bool IsOrderExpired(Order order)
    {
        return order.ExpiresAt < _timeProvider.GetCurrentTime();
    }
}
```

#### Testing with ITimeProvider

```csharp
public class TestTimeProvider : ITimeProvider
{
    public DateTimeOffset CurrentTime { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset GetCurrentTime() => CurrentTime;

    public Task<Result> WaitAsync(TimeSpan timeSpan, CancellationToken cancellationToken = default)
    {
        // In tests, we can simulate time passing instantly
        CurrentTime = CurrentTime.Add(timeSpan);
        return Task.FromResult(Result.Success());
    }
}

[Test]
public void CreateOrder_ShouldSetCorrectExpirationDate()
{
    // Arrange
    var timeProvider = new TestTimeProvider 
    { 
        CurrentTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero) 
    };
    var orderService = new OrderService(timeProvider);

    // Act
    var order = orderService.CreateOrder(new CreateOrderRequest());

    // Assert
    order.ExpiresAt.Should().Be(new DateTimeOffset(2024, 1, 31, 0, 0, 0, TimeSpan.Zero));
}
```

### IRetryService - Robust Error Handling

The `IRetryService` provides configurable retry logic with exponential backoff for handling transient failures.

#### Interface

```csharp
public interface IRetryService
{
    Task<Result> TryCallAsync(
        Func<CancellationToken, Task> callback, 
        TimeSpan timeout, 
        int retryCount = 3);
}
```

#### Usage Examples

```csharp
public class PaymentService
{
    private readonly IRetryService _retryService;
    private readonly IPaymentGateway _paymentGateway;

    public PaymentService(IRetryService retryService, IPaymentGateway paymentGateway)
    {
        _retryService = retryService;
        _paymentGateway = paymentGateway;
    }

    public async Task<Result> ProcessPaymentAsync(Payment payment)
    {
        // Retry payment processing up to 3 times with 30-second timeout
        return await _retryService.TryCallAsync(
            async cancellationToken =>
            {
                await _paymentGateway.ChargeAsync(payment, cancellationToken);
            },
            timeout: TimeSpan.FromSeconds(30),
            retryCount: 3
        );
    }

    public async Task<Result<User>> FetchUserDataAsync(Guid userId)
    {
        var result = await _retryService.TryCallAsync(
            async cancellationToken =>
            {
                var userData = await _externalApi.GetUserAsync(userId, cancellationToken);
                if (userData == null)
                    throw new UserNotFoundException($"User {userId} not found");
            },
            timeout: TimeSpan.FromSeconds(10),
            retryCount: 5
        );

        return result.IsSuccess 
            ? Result.Success(userData)
            : Result.Failure<User>(result.Error);
    }
}
```

#### Custom Retry Policies

```csharp
public class CustomRetryService
{
    public async Task<Result> RetryWithBackoffAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        TimeSpan baseDelay = default)
    {
        baseDelay = baseDelay == default ? TimeSpan.FromMilliseconds(100) : baseDelay;
        
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                await operation();
                return Result.Success();
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                var delay = TimeSpan.FromTicks(baseDelay.Ticks * (long)Math.Pow(2, attempt));
                await Task.Delay(delay);
            }
            catch (Exception ex)
            {
                return Error.FromException("RETRY_EXHAUSTED", ErrorType.Error, ex);
            }
        }
        
        return Result.Success();
    }
}
```

### ITaskProvider - Thread-Safe Operations

The `ITaskProvider` offers abstractions for task delays and resource locking.

#### Interface

```csharp
public interface ITaskProvider : IDisposable
{
    Task DelayAsync(TimeSpan delay, CancellationToken token);
    Task LockAsync(CancellationToken token);
    Task LockReleaseAsync();
    Task ReleaseSyncLockAsync();
    // ... other methods
}
```

#### Usage Examples

```csharp
public class ResourceManager
{
    private readonly ITaskProvider _taskProvider;
    private readonly Dictionary<string, object> _resources = new();

    public ResourceManager(ITaskProvider taskProvider)
    {
        _taskProvider = taskProvider;
    }

    public async Task<T> AccessResourceSafelyAsync<T>(
        string resourceKey, 
        Func<Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        await _taskProvider.LockAsync(cancellationToken);
        try
        {
            // Simulate processing delay
            await _taskProvider.DelayAsync(TimeSpan.FromMilliseconds(100), cancellationToken);
            
            return await operation();
        }
        finally
        {
            await _taskProvider.LockReleaseAsync();
        }
    }

    public async Task ProcessBatchAsync<T>(
        IEnumerable<T> items,
        Func<T, Task> processor,
        TimeSpan delayBetweenItems = default)
    {
        foreach (var item in items)
        {
            await processor(item);
            
            if (delayBetweenItems > TimeSpan.Zero)
            {
                await _taskProvider.DelayAsync(delayBetweenItems, CancellationToken.None);
            }
        }
    }
}
```

### JobProcessor<T> - Background Job Processing

The `JobProcessor<T>` provides high-performance background job processing using .NET channels.

#### Usage Examples

```csharp
public class EmailService
{
    private readonly JobProcessor<EmailJob> _emailProcessor;

    public EmailService(ILogger<JobProcessor<EmailJob>> logger)
    {
        _emailProcessor = new JobProcessor<EmailJob>(logger);
        _emailProcessor.Init(ProcessEmailAsync);
    }

    public async Task QueueEmailAsync(string to, string subject, string body)
    {
        var emailJob = new EmailJob(to, subject, body);
        await _emailProcessor.EnqueueAsync(emailJob);
    }

    private async Task ProcessEmailAsync(EmailJob job)
    {
        try
        {
            // Simulate email sending
            await Task.Delay(1000);
            Console.WriteLine($"Email sent to {job.To}: {job.Subject}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email: {ex.Message}");
        }
    }
}

public record EmailJob(string To, string Subject, string Body);
```

#### Advanced Job Processing

```csharp
public class OrderProcessingService
{
    private readonly JobProcessor<OrderProcessingJob> _orderProcessor;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentService _paymentService;

    public OrderProcessingService(
        ILogger<JobProcessor<OrderProcessingJob>> logger,
        IOrderRepository orderRepository,
        IPaymentService paymentService)
    {
        _orderRepository = orderRepository;
        _paymentService = paymentService;
        
        _orderProcessor = new JobProcessor<OrderProcessingJob>(logger);
        _orderProcessor.Init(ProcessOrderAsync);
    }

    public async Task EnqueueOrderProcessingAsync(Guid orderId)
    {
        await _orderProcessor.EnqueueAsync(new OrderProcessingJob(orderId));
    }

    private async Task ProcessOrderAsync(OrderProcessingJob job)
    {
        var order = await _orderRepository.GetByIdAsync(job.OrderId);
        if (order == null) return;

        // Process payment
        var paymentResult = await _paymentService.ProcessPaymentAsync(order.Payment);
        if (!paymentResult.IsSuccess)
        {
            order.MarkAsFailed(paymentResult.Error.Message);
            await _orderRepository.UpdateAsync(order);
            return;
        }

        // Update inventory
        foreach (var item in order.Items)
        {
            await UpdateInventoryAsync(item);
        }

        order.MarkAsCompleted();
        await _orderRepository.UpdateAsync(order);
    }
}

public record OrderProcessingJob(Guid OrderId);
```

## Advanced Patterns

### Service Layer with Common Abstractions

```csharp
public class UserService
{
    private readonly ITimeProvider _timeProvider;
    private readonly IRetryService _retryService;
    private readonly IUserRepository _userRepository;
    private readonly JobProcessor<UserNotificationJob> _notificationProcessor;

    public UserService(
        ITimeProvider timeProvider,
        IRetryService retryService,
        IUserRepository userRepository,
        ILogger<JobProcessor<UserNotificationJob>> logger)
    {
        _timeProvider = timeProvider;
        _retryService = retryService;
        _userRepository = userRepository;
        _notificationProcessor = new JobProcessor<UserNotificationJob>(logger);
        _notificationProcessor.Init(SendNotificationAsync);
    }

    public async Task<Result<User>> CreateUserAsync(CreateUserRequest request)
    {
        var now = _timeProvider.GetCurrentTime();
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            CreatedAt = now,
            LastLoginAt = now
        };

        // Save with retry logic
        var saveResult = await _retryService.TryCallAsync(
            async ct => await _userRepository.SaveAsync(user, ct),
            timeout: TimeSpan.FromSeconds(5),
            retryCount: 3
        );

        if (!saveResult.IsSuccess)
            return Result.Failure<User>(saveResult.Error);

        // Queue welcome notification
        await _notificationProcessor.EnqueueAsync(
            new UserNotificationJob(user.Id, NotificationType.Welcome)
        );

        return Result.Success(user);
    }

    private async Task SendNotificationAsync(UserNotificationJob job)
    {
        // Implementation for sending notifications
        await Task.Delay(100); // Simulate work
    }
}

public record UserNotificationJob(Guid UserId, NotificationType Type);
public enum NotificationType { Welcome, PasswordReset, AccountUpdate }
```

### Testing Strategy

```csharp
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepository;
    private readonly TestTimeProvider _timeProvider;
    private readonly Mock<IRetryService> _retryService;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepository = new Mock<IUserRepository>();
        _timeProvider = new TestTimeProvider();
        _retryService = new Mock<IRetryService>();
        _userService = new UserService(_timeProvider, _retryService.Object, _userRepository.Object, Mock.Of<ILogger<JobProcessor<UserNotificationJob>>>());
    }

    [Test]
    public async Task CreateUser_WithValidRequest_ShouldSetCorrectTimestamps()
    {
        // Arrange
        var fixedTime = new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero);
        _timeProvider.CurrentTime = fixedTime;
        
        _retryService
            .Setup(x => x.TryCallAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<TimeSpan>(), It.IsAny<int>()))
            .ReturnsAsync(Result.Success());

        var request = new CreateUserRequest { Email = "test@example.com" };

        // Act
        var result = await _userService.CreateUserAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CreatedAt.Should().Be(fixedTime);
        result.Value.LastLoginAt.Should().Be(fixedTime);
    }
}
```

## Best Practices

### Time Operations
- Always use `ITimeProvider` instead of `DateTime.Now` or `DateTimeOffset.Now`
- Prefer `DateTimeOffset` over `DateTime` for time zone awareness
- Use the `WaitAsync` method for delays that need to respect cancellation tokens

### Retry Logic
- Use `IRetryService` for external API calls and database operations
- Set appropriate timeouts based on operation type
- Consider exponential backoff for high-traffic scenarios
- Log retry attempts for debugging

### Background Processing
- Use `JobProcessor<T>` for fire-and-forget operations
- Keep job handlers lightweight and fast
- Implement proper error handling in job processors
- Consider job persistence for critical operations

### Resource Management
- Always dispose of `ITaskProvider` and `JobProcessor<T>` instances
- Use `using` statements or dependency injection container for lifecycle management
- Implement proper cancellation token support

## Performance Considerations

- **JobProcessor**: Uses unbounded channels for maximum throughput
- **Retry Service**: Implements efficient exponential backoff
- **Task Provider**: Minimal overhead for lock operations
- **Time Provider**: Direct system calls with no caching overhead

## Thread Safety

All implementations in this package are thread-safe:
- `TimeProvider`: Stateless operations
- `RetryService`: No shared state between calls
- `TaskProvider`: Uses thread-safe synchronization primitives
- `JobProcessor<T>`: Built on thread-safe channels
