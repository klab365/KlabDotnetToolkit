# Klab.Toolkit.Results

## Purpose

This project is a part of the Klab.Toolkit solution and provides a robust error handling mechanism using the Result pattern. The Result pattern helps to avoid exceptions by representing the outcome of operations that can either succeed or fail. This approach makes error handling explicit and more predictable.

## Core Features

- **`Result<T>`**: A generic class that represents the result of an operation with a value. It can be either a success (containing a value) or a failure (containing an error).
- **`Result`**: A non-generic class that represents the result of an operation without a value. It can be either a success or a failure.
- **`Error`**: A record that represents an error with properties like Code, Message, Advice, Type, and optional StackTrace.
- **`ErrorType`**: An enum defining error severity levels: Info, Warning, Error, Fatal.
- **`ResultExtensions`**: A comprehensive set of extension methods that implement functional programming concepts like `Map`, `Bind`, `OnSuccess`, `OnFailure`, and `Match`.

## When to Use

Use the Result pattern when:
- You want to avoid throwing exceptions for expected error conditions
- You need explicit error handling in your application flow
- You're building a functional programming style codebase
- You want to chain operations that might fail
- You need better testability and predictability in error scenarios

## Basic Usage

### Creating Results

```csharp
// Success cases
Result<int> successWithValue = Result.Success(42);
Result success = Result.Success();

// Failure cases
Error error = Error.Create("USER_NOT_FOUND", "User with ID 123 was not found", "Check if the user ID is correct");
Result<User> failureWithValue = Result.Failure<User>(error);
Result failure = Result.Failure(error);

// Using implicit conversions
Result<string> fromValue = "Hello World"; // Implicitly converted to Success
Result<int> fromError = Error.Create("INVALID_INPUT", "Input is invalid"); // Implicitly converted to Failure
```

### Basic Checking

```csharp
var result = GetUser(userId);

if (result.IsSuccess)
{
    Console.WriteLine($"User found: {result.Value.Name}");
}
else
{
    Console.WriteLine($"Error: {result.Error.Message}");
}

// Or using implicit bool conversion
if (result)
{
    Console.WriteLine("Operation succeeded");
}
```

## Extension Methods with Examples

### Map - Transform Success Values

Use `Map` when you want to transform the value inside a successful Result without changing the Result wrapper.

```csharp
// Transform a successful result
Result<int> number = Result.Success(5);
Result<string> text = number.Map(x => $"Number: {x}");
// Result: Success("Number: 5")

// Chain multiple transformations
Result<string> result = Result.Success(10)
    .Map(x => x * 2)        // 20
    .Map(x => x + 5)        // 25
    .Map(x => x.ToString()); // "25"

// Async version
Result<User> user = await GetUserAsync(id)
    .MapAsync(async u => await EnrichUserDataAsync(u));
```

**When to use**: Transform data when the operation cannot fail (pure transformations).

### Bind - Chain Operations That Can Fail

Use `Bind` when you want to chain operations that themselves return Results.

```csharp
// Chain operations that can fail
Result<User> result = Result.Success(userId)
    .Bind(id => GetUser(id))                    // Returns Result<User>
    .Bind(user => ValidateUser(user))           // Returns Result<User>
    .Bind(user => UpdateLastLogin(user));       // Returns Result<User>

// Mixed Result types
Result<string> emailResult = Result.Success(userId)
    .Bind(id => GetUser(id))                    // Result<User>
    .Bind(user => GetUserEmail(user));          // Result<string>

// From Result to Result<T>
Result processResult = ValidateInput(data);
Result<ProcessedData> finalResult = processResult
    .Bind(() => ProcessData(data));             // Only executes if validation succeeds

// Async version
Result<Order> order = await Result.Success(customerId)
    .BindAsync(async id => await GetCustomerAsync(id))
    .BindAsync(async customer => await CreateOrderAsync(customer));
```

**When to use**: Chain operations where each step can fail and you want to stop on the first failure.

### OnSuccess - Execute Side Effects on Success

Use `OnSuccess` when you want to perform actions (logging, notifications, etc.) without modifying the Result.

```csharp
// Simple side effect
Result<User> result = GetUser(id)
    .OnSuccess(user => Console.WriteLine($"User {user.Name} retrieved successfully"));

// Multiple side effects
Result<Order> orderResult = CreateOrder(orderData)
    .OnSuccess(order => logger.LogInformation($"Order {order.Id} created"))
    .OnSuccess(order => SendConfirmationEmail(order.CustomerEmail))
    .OnSuccess(order => UpdateInventory(order.Items));

// Async side effects
Result<Payment> payment = await ProcessPayment(paymentData)
    .OnSuccessAsync(async p => await SendReceiptAsync(p.CustomerEmail));
```

**When to use**: Logging, notifications, audit trails, or any action that should only happen on success.

### OnFailure - Execute Side Effects on Failure

Use `OnFailure` for error handling, logging, or cleanup operations.

```csharp
// Error logging
Result<User> result = GetUser(id)
    .OnFailure(error => logger.LogError($"Failed to get user: {error.Message}"));

// Multiple failure handling
Result<Order> orderResult = ProcessOrder(orderData)
    .OnFailure(error => logger.LogError($"Order processing failed: {error.Message}"))
    .OnFailure(error => NotifyAdministrator(error))
    .OnFailure(error => UpdateErrorMetrics(error.Type));

// Async failure handling
Result<Payment> payment = await ProcessPayment(paymentData)
    .OnFailureAsync(async error => await SendFailureNotificationAsync(error));
```

**When to use**: Error logging, cleanup operations, notifications, metrics collection.

### Match - Handle Both Success and Failure Cases

Use `Match` when you need to handle both success and failure cases and return a specific result.

```csharp
// Return different types based on outcome
string message = GetUser(id).Match(
    onSuccess: user => $"Welcome, {user.Name}!",
    onFailure: error => $"Error: {error.Message}"
);

// Execute different actions
GetUser(id).Match(
    onSuccess: user => RedirectToUserDashboard(user),
    onFailure: error => ShowErrorPage(error)
);

// Convert to HTTP responses
IActionResult response = ProcessOrder(orderData).Match(
    onSuccess: order => Ok(new { OrderId = order.Id, Status = "Created" }),
    onFailure: error => BadRequest(new { Error = error.Message, Code = error.Code })
);
```

**When to use**: Converting Results to other types, handling both cases explicitly, creating responses.

### ToResult - Wrap Values

Convert regular values to Results.

```csharp
// Wrap a value
int number = 42;
Result<int> result = number.ToResult(); // Success(42)

// Useful in method chains
Result<string> processed = GetData()
    .ToResult()
    .Map(data => ProcessData(data));
```

**When to use**: When you need to convert a regular value to a Result for chaining.

### Unwrap - Extract Values (Use with Caution)

Extract the value from a Result, throwing an exception if it's a failure.

```csharp
// Only use when you're certain the result is successful
Result<int> result = Result.Success(42);
int value = result.Unwrap(); // Returns 42

// This will throw an InvalidOperationException
Result<int> failure = Result.Failure<int>(Error.Create("ERROR", "Something went wrong"));
int value = failure.Unwrap(); // Throws exception
```

**When to use**: Rarely. Only when you're absolutely certain the Result is successful. Prefer `Match` or checking `IsSuccess`.

## Practical Examples

### Service Layer Pattern

```csharp
public class UserService
{
    public async Task<Result<User>> CreateUserAsync(CreateUserRequest request)
    {
        return await Result.Success(request)
            .Bind(req => ValidateRequest(req))
            .BindAsync(async req => await CheckEmailUniqueness(req.Email))
            .BindAsync(async req => await HashPassword(req.Password))
            .BindAsync(async req => await SaveUserAsync(req))
            .OnSuccessAsync(async user => await SendWelcomeEmailAsync(user))
            .OnFailure(error => logger.LogError($"User creation failed: {error.Message}"));
    }

    private Result<CreateUserRequest> ValidateRequest(CreateUserRequest request)
    {
        if (string.IsNullOrEmpty(request.Email))
            return Error.Create("INVALID_EMAIL", "Email is required");

        if (request.Password.Length < 8)
            return Error.Create("WEAK_PASSWORD", "Password must be at least 8 characters");

        return request;
    }
}
```

### API Controller Pattern

```csharp
[ApiController]
public class UsersController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        var result = await userService.CreateUserAsync(request);

        return result.Match(
            onSuccess: user => Created($"/users/{user.Id}", new { user.Id, user.Email }),
            onFailure: error => error.Type switch
            {
                ErrorType.Error => BadRequest(new { error.Code, error.Message }),
                ErrorType.Fatal => StatusCode(500, new { Message = "Internal server error" }),
                _ => BadRequest(new { error.Code, error.Message })
            }
        );
    }
}
```

### Data Processing Pipeline

```csharp
public async Task<Result<ProcessedData>> ProcessDataPipeline(RawData data)
{
    return await Result.Success(data)
        .Bind(d => ValidateData(d))
        .BindAsync(async d => await EnrichData(d))
        .Map(d => TransformData(d))
        .BindAsync(async d => await SaveProcessedData(d))
        .OnSuccess(d => logger.LogInformation($"Processed {d.RecordCount} records"))
        .OnFailure(error => logger.LogError($"Pipeline failed at: {error.Code}"));
}
```

## Error Handling Best Practices

1. **Use specific error codes**: Make error codes meaningful and consistent across your application.
2. **Provide helpful messages**: Include context about what went wrong and how to fix it.
3. **Use appropriate error types**: Choose the right `ErrorType` (Info, Warning, Error, Fatal).
4. **Avoid exceptions for expected failures**: Use Results for business logic failures.
5. **Chain operations efficiently**: Use `Bind` for operations that can fail, `Map` for transformations.
6. **Handle errors explicitly**: Use `Match` or check `IsSuccess` rather than ignoring potential failures.

## Common Anti-Patterns to Avoid

```csharp
// ❌ Don't do this - ignoring failures
var result = GetUser(id);
var name = result.Value.Name; // Will throw if result failed

// ✅ Do this instead
var name = GetUser(id).Match(
    onSuccess: user => user.Name,
    onFailure: error => "Unknown User"
);

// ❌ Don't do this - mixing exceptions with Results
public Result<User> GetUser(int id)
{
    if (id <= 0) throw new ArgumentException("Invalid ID");
    // ... rest of method
}

// ✅ Do this instead
public Result<User> GetUser(int id)
{
    if (id <= 0) return Error.Create("INVALID_ID", "User ID must be positive");
    // ... rest of method
}
```

