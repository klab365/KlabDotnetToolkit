# Klab.Toolkit.Event

## Overview

The `Klab.Toolkit.Event` package is a comprehensive event-driven communication system that enables decoupled, scalable, and maintainable applications. It provides three main communication patterns: **Event Publishing/Subscribing**, **Request/Response**, and **Stream Request/Response**. This package promotes loose coupling between components while maintaining high performance and flexibility.

## Purpose

The primary purpose of the `Klab.Toolkit.Event` package is to facilitate various forms of asynchronous communication within applications:

- **Event-Driven Architecture**: Publish events when something happens and let multiple handlers react independently
- **Request/Response Pattern**: Send commands and queries with guaranteed single responses (similar to MediatR)
- **Streaming Responses**: Handle requests that return multiple values over time
- **Decoupled Communication**: Components can communicate without direct dependencies

## Core Architecture

### Key Components

- **`IEventBus`**: Central messaging hub for all communication patterns
- **`EventBase`**: Abstract base class for all events with built-in ID and timestamp
- **`IEventHandler<T>`**: Interface for handling specific event types
- **`IRequest<T>` & `IRequestHandler<T,R>`**: Request/response pattern interfaces
- **`IStreamRequest<T>` & `IStreamRequestHandler<T,R>`**: Streaming response interfaces
- **`IEventQueue`**: Pluggable message queue interface (default: in-memory)

### Communication Patterns

1. **Events**: Fire-and-forget notifications that can have multiple handlers
2. **Requests**: Commands/queries that expect exactly one response
3. **Stream Requests**: Requests that return multiple responses over time

## Setup and Configuration

### Basic Setup

```csharp
// In Program.cs or Startup.cs
var builder = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        // Register the event module with default configuration
        services.UseEventModule();

        // Register event handlers
        services.AddEventSubsribtion<UserRegisteredEvent, UserWelcomeEmailHandler>();
        services.AddEventSubsribtion<UserRegisteredEvent, UserAnalyticsHandler>();

        // Register request handlers
        services.AddRequestResponseHandler<GetUserQuery, Result<User>, GetUserQueryHandler>();
        services.AddRequestResponseHandler<CreateUserCommand, Result<Guid>, CreateUserCommandHandler>();

        // Register stream handlers
        services.AddStreamRequestResponseHandler<GetUserActivityStream, UserActivity, GetUserActivityStreamHandler>();
    });

var host = builder.Build();
await host.StartAsync();
```

### Advanced Configuration

```csharp
services.UseEventModule(config =>
{
    // Use custom event queue implementation
    config.EventQueueType = typeof(MyCustomEventQueue);
    config.EventQueueLifetime = ServiceLifetime.Scoped;

    // Configure event logging
    config.ShouldLogEvents = true;
    config.EventLogFilePath = "logs/events.json";
});
```

## Event Publishing and Subscribing

### Creating Events

```csharp
// Simple event
public sealed record UserRegisteredEvent(Guid UserId, string Email, DateTime RegisteredAt) : EventBase;

// Event with sensitive data (password won't be logged)
public sealed record UserLoginAttemptEvent(string Email, bool Success) : EventBase
{
    [JsonIgnore]
    public string Password { get; init; } = string.Empty;
}

// Event with custom properties
public sealed record OrderCreatedEvent : EventBase
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal TotalAmount { get; init; }
    public List<OrderItem> Items { get; init; } = new();

    // Override default timestamp with business logic
    public override DateTime CreatedAt => OrderPlacedAt;
    public DateTime OrderPlacedAt { get; init; } = DateTime.UtcNow;
}
```

### Event Handlers

```csharp
// Class-based event handler
public class UserWelcomeEmailHandler : IEventHandler<UserRegisteredEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<UserWelcomeEmailHandler> _logger;

    public UserWelcomeEmailHandler(IEmailService emailService, ILogger<UserWelcomeEmailHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            await _emailService.SendWelcomeEmailAsync(notification.Email, cancellationToken);
            _logger.LogInformation("Welcome email sent to {Email}", notification.Email);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", notification.Email);
            return Error.FromException("EMAIL_SEND_FAILED", ErrorType.Error, ex);
        }
    }
}

// Another handler for the same event
public class UserAnalyticsHandler : IEventHandler<UserRegisteredEvent>
{
    private readonly IAnalyticsService _analytics;

    public UserAnalyticsHandler(IAnalyticsService analytics)
    {
        _analytics = analytics;
    }

    public async Task<Result> Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        await _analytics.TrackUserRegistrationAsync(notification.UserId);
        return Result.Success();
    }
}
```

### Publishing Events

```csharp
public class UserService
{
    private readonly IEventBus _eventBus;

    public UserService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task<Result<User>> RegisterUserAsync(RegisterUserRequest request)
    {
        // Business logic
        var user = new User(request.Email, request.Name);
        await SaveUserAsync(user);

        // Publish event - fire and forget
        var userRegisteredEvent = new UserRegisteredEvent(user.Id, user.Email, DateTime.UtcNow);
        await _eventBus.PublishAsync(userRegisteredEvent);

        return Result.Success(user);
    }
}
```

### Local Function Subscriptions

```csharp
public class OrderService
{
    private readonly IEventBus _eventBus;

    public OrderService(IEventBus eventBus)
    {
        _eventBus = eventBus;

        // Subscribe with local functions for simple scenarios
        _eventBus.Subscribe<OrderCreatedEvent>(HandleOrderCreated);
        _eventBus.Subscribe<OrderCancelledEvent>(HandleOrderCancelled);
    }

    private async Task<Result> HandleOrderCreated(OrderCreatedEvent orderEvent, CancellationToken ct)
    {
        // Update inventory
        await UpdateInventoryAsync(orderEvent.Items);
        return Result.Success();
    }

    private async Task<Result> HandleOrderCancelled(OrderCancelledEvent orderEvent, CancellationToken ct)
    {
        // Restore inventory
        await RestoreInventoryAsync(orderEvent.Items);
        return Result.Success();
    }
}
```

## Request/Response Pattern

The request/response pattern is perfect for commands and queries where you need exactly one response.

### Creating Requests and Handlers

```csharp
// Query example
public sealed record GetUserQuery(Guid UserId) : IRequest<Result<User>>;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, Result<User>>
{
    private readonly IUserRepository _userRepository;

    public GetUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<User>> HandleAsync(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        return user is not null
            ? Result.Success(user)
            : Result.Failure<User>(Error.Create("USER_NOT_FOUND", $"User with ID {request.UserId} not found"));
    }
}

// Command example
public sealed record CreateOrderCommand(Guid CustomerId, List<OrderItem> Items) : IRequest<Result<Guid>>;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<Guid>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryService _inventoryService;

    public CreateOrderCommandHandler(IOrderRepository orderRepository, IInventoryService inventoryService)
    {
        _orderRepository = orderRepository;
        _inventoryService = inventoryService;
    }

    public async Task<Result<Guid>> HandleAsync(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate inventory
        var inventoryResult = await _inventoryService.CheckAvailabilityAsync(request.Items);
        if (!inventoryResult.IsSuccess)
            return Result.Failure<Guid>(inventoryResult.Error);

        // Create order
        var order = new Order(request.CustomerId, request.Items);
        await _orderRepository.SaveAsync(order);

        return Result.Success(order.Id);
    }
}
```

### Sending Requests

```csharp
public class OrderController : ControllerBase
{
    private readonly IEventBus _eventBus;

    public OrderController(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var query = new GetUserQuery(id);
        var result = await _eventBus.SendAsync(query);

        return result.Match(
            onSuccess: user => Ok(user),
            onFailure: error => NotFound(new { error.Code, error.Message })
        );
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        var command = new CreateOrderCommand(request.CustomerId, request.Items);
        var result = await _eventBus.SendAsync(command);

        return result.Match(
            onSuccess: orderId => Created($"/orders/{orderId}", new { OrderId = orderId }),
            onFailure: error => BadRequest(new { error.Code, error.Message })
        );
    }
}
```

## Stream Request/Response Pattern

Use streaming for scenarios where you need to return multiple values over time, such as real-time data feeds or paginated results.

### Creating Stream Requests and Handlers

```csharp
// Stream request for real-time data
public sealed record GetUserActivityStreamRequest(Guid UserId, DateTime StartDate) : IStreamRequest<UserActivity>;

public class GetUserActivityStreamHandler : IStreamRequestHandler<GetUserActivityStreamRequest, UserActivity>
{
    private readonly IUserActivityRepository _repository;

    public GetUserActivityStreamHandler(IUserActivityRepository repository)
    {
        _repository = repository;
    }

    public async IAsyncEnumerable<UserActivity> HandleAsync(
        GetUserActivityStreamRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Stream historical data first
        await foreach (var activity in _repository.GetActivitiesAsync(request.UserId, request.StartDate, cancellationToken))
        {
            yield return activity;
        }

        // Then stream real-time updates
        await foreach (var activity in _repository.GetRealTimeActivitiesAsync(request.UserId, cancellationToken))
        {
            yield return activity;
        }
    }
}

// Paginated results example
public sealed record GetOrdersPagedRequest(int PageSize, string? ContinuationToken) : IStreamRequest<Order>;

public class GetOrdersPagedHandler : IStreamRequestHandler<GetOrdersPagedRequest, Order>
{
    private readonly IOrderRepository _repository;

    public GetOrdersPagedHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async IAsyncEnumerable<Order> HandleAsync(
        GetOrdersPagedRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string? continuationToken = request.ContinuationToken;

        do
        {
            var page = await _repository.GetPageAsync(request.PageSize, continuationToken, cancellationToken);

            foreach (var order in page.Items)
            {
                yield return order;
            }

            continuationToken = page.ContinuationToken;
        }
        while (continuationToken is not null && !cancellationToken.IsCancellationRequested);
    }
}
```

### Consuming Streams

```csharp
public class ActivityService
{
    private readonly IEventBus _eventBus;

    public ActivityService(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    public async Task ProcessUserActivityAsync(Guid userId)
    {
        var streamRequest = new GetUserActivityStreamRequest(userId, DateTime.UtcNow.AddDays(-30));

        await foreach (var activity in _eventBus.Stream(streamRequest))
        {
            // Process each activity as it comes
            await ProcessActivityAsync(activity);
        }
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        var orders = new List<Order>();
        var streamRequest = new GetOrdersPagedRequest(PageSize: 100, ContinuationToken: null);

        await foreach (var order in _eventBus.Stream(streamRequest))
        {
            orders.Add(order);
        }

        return orders;
    }
}
```

## Advanced Scenarios

### Error Handling in Handlers

```csharp
public class RobustEventHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<RobustEventHandler> _logger;

    public async Task<Result> Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var paymentResult = await _paymentService.ProcessPaymentAsync(notification.OrderId);

            if (!paymentResult.IsSuccess)
            {
                _logger.LogWarning("Payment failed for order {OrderId}: {Error}",
                    notification.OrderId, paymentResult.Error.Message);
                return paymentResult;
            }

            return Result.Success();
        }
        catch (PaymentServiceException ex)
        {
            _logger.LogError(ex, "Payment service error for order {OrderId}", notification.OrderId);
            return Error.Create("PAYMENT_SERVICE_ERROR", ex.Message, "Retry payment later", ErrorType.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing payment for order {OrderId}", notification.OrderId);
            return Error.FromException("UNEXPECTED_ERROR", ErrorType.Fatal, ex);
        }
    }
}
```

### Custom Event Queue Implementation

```csharp
// Example: Redis-based event queue
public class RedisEventQueue : IEventQueue
{
    private readonly IDatabase _database;
    private readonly string _queueName;

    public RedisEventQueue(IConnectionMultiplexer redis, string queueName = "events")
    {
        _database = redis.GetDatabase();
        _queueName = queueName;
    }

    public async Task EnqueueAsync(EventBase @event, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(@event, @event.GetType());
        await _database.ListLeftPushAsync(_queueName, json);
    }

    public async Task<EventBase?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        var json = await _database.ListRightPopAsync(_queueName);
        if (!json.HasValue) return null;

        // Deserialize with type information (implementation depends on your JSON setup)
        return DeserializeEvent(json);
    }

    public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return _database.ListLengthAsync(_queueName).AsTask();
    }
}

// Register custom queue
services.UseEventModule(config =>
{
    config.EventQueueType = typeof(RedisEventQueue);
});
```

### Conditional Event Processing

```csharp
public class ConditionalEventHandler : IEventHandler<UserRegisteredEvent>
{
    private readonly IFeatureToggleService _featureToggles;
    private readonly IEmailService _emailService;

    public async Task<Result> Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        // Only send emails if feature is enabled
        if (!await _featureToggles.IsEnabledAsync("WelcomeEmails"))
        {
            return Result.Success(); // Skip processing but return success
        }

        // Only for certain user types
        if (notification.Email.EndsWith("@test.com"))
        {
            return Result.Success(); // Skip test users
        }

        await _emailService.SendWelcomeEmailAsync(notification.Email);
        return Result.Success();
    }
}
```

## Best Practices

### Event Design
- **Events should be immutable**: Use records with init-only properties
- **Include relevant context**: Add enough information for handlers to work independently
- **Use past tense names**: `UserRegistered`, `OrderCreated`, `PaymentProcessed`
- **Avoid sensitive data**: Use `[JsonIgnore]` for passwords, tokens, etc.

### Handler Design
- **Keep handlers focused**: One handler should do one thing
- **Handle errors gracefully**: Return appropriate Result types with meaningful errors
- **Make handlers idempotent**: Safe to run multiple times with same input
- **Use dependency injection**: Don't create dependencies manually

### Performance Considerations
- **Use appropriate lifetimes**: Singleton for stateless services, Scoped for EF contexts
- **Consider async patterns**: Use ConfigureAwait(false) for library code
- **Monitor queue depths**: Implement health checks for event queue
- **Handle backpressure**: Consider circuit breakers for failing handlers

### Testing
```csharp
[Fact]
public async Task UserRegisteredEvent_ShouldSendWelcomeEmail()
{
    // Arrange
    var emailService = new Mock<IEmailService>();
    var handler = new UserWelcomeEmailHandler(emailService.Object, Mock.Of<ILogger<UserWelcomeEmailHandler>>());
    var @event = new UserRegisteredEvent(Guid.NewGuid(), "test@example.com", DateTime.UtcNow);

    // Act
    var result = await handler.Handle(@event, CancellationToken.None);

    // Assert
    result.IsSuccess.Should().BeTrue();
    emailService.Verify(x => x.SendWelcomeEmailAsync("test@example.com", It.IsAny<CancellationToken>()), Times.Once);
}
```

## Migration from Other Libraries

### From MediatR
The request/response pattern is very similar to MediatR:

```csharp
// MediatR
public class GetUserQuery : IRequest<User> { ... }
public class GetUserHandler : IRequestHandler<GetUserQuery, User> { ... }

// Klab.Toolkit.Event
public record GetUserQuery : IRequest<Result<User>> { ... }
public class GetUserHandler : IRequestHandler<GetUserQuery, Result<User>> { ... }
```

Key differences:
- Return types should be wrapped in `Result<T>` for better error handling
- Use `SendAsync` instead of `Send`
- Registration uses `AddRequestResponseHandler` instead of `AddMediatR`

## Troubleshooting

### Common Issues

1. **Events not being processed**
   - Ensure `EventProcesserJob` is registered as hosted service
   - Check if event queue is working properly
   - Verify handlers are registered in DI container

2. **Request handlers not found**
   - Ensure handler is registered with `AddRequestResponseHandler`
   - Check that request implements correct interface
   - Verify handler implements `IRequestHandler<TRequest, TResponse>`

3. **Performance issues**
   - Monitor event queue depth
   - Check for slow handlers blocking processing
   - Consider using background services for heavy operations

4. **Memory leaks**
   - Unsubscribe from local function subscriptions when done
   - Check for circular dependencies in DI container
   - Monitor event queue growth


