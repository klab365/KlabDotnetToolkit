# Klab.Toolkit.Results

## Purpose

This project is a part of the Klab.Toolkit solution and provides a robust error handling mechanism using the Result pattern. The Result pattern helps to avoid exceptions by representing the outcome of operations that can either succeed or fail. This approach makes error handling explicit and more predictable.

## Core Features

- **`Result<T>`**: A generic class that represents the result of an operation with a value. It can be either a success (containing a value) or a failure (containing an error).
- **`Result`**: A non-generic class that represents the result of an operation without a value. It can be either a success or a failure.
- **`Error`**: A record that represents an error with properties like Code, Message, Advice, optional Exception, and support for nested errors.
- **`ResultExtensions`**: A comprehensive set of extension methods that implement functional programming concepts like `Map`, `Bind`, `OnSuccess`, `OnFailure`, `Do`, and `Match`.

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

// Using explicit conversions
Result<string> fromValue = Result.Success("Hello World");
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

// Or check result success using IsSuccess property
if (result.IsSuccess)
{
    Console.WriteLine("Operation succeeded");
}
```

## Error Handling

The `Error` class is a fundamental part of the Result pattern, providing rich error information with support for hierarchical error structures.

### Error Creation

```csharp
// Basic error creation
Error error = Error.Create("USER_NOT_FOUND", "User with ID 123 was not found");

// Error with advice
Error detailedError = Error.Create(
    code: "VALIDATION_FAILED",
    message: "Email format is invalid",
    advice: "Please provide a valid email address with @ symbol");

// Convenience methods for common error types
Error warning = Error.Warning("WARN001", "This is a warning", "Consider reviewing this");
Error critical = Error.Critical("CRIT001", "System failure", "Immediate action required");
Error failure = Error.Failure("ERR001", "Operation failed");
Error debug = Error.Debug("DEBUG001", "Debug information");
```

### Error from Exception

```csharp
try
{
    // Some operation that might throw
    DoSomethingRisky();
}
catch (ArgumentException ex)
{
    // Create error from exception with defaults
    Error error = Error.FromException(ex);
    // Code: "ArgumentException", Message: ex.Message

    // Create error from exception with custom details
    Error customError = Error.FromException(
        exception: ex,
        code: "ARG001",
        advice: "Check your input parameters");

    return Result.Failure<User>(customError);
}
```

### Multiple Errors - Composite Error Handling

When you have multiple errors from validation or processing, you can combine them into a single Error:

#### Using Error.Multiple() - Simple Approach

```csharp
public Result<User> ValidateUser(CreateUserRequest request)
{
    var errors = new List<Error>();

    if (string.IsNullOrEmpty(request.Name))
        errors.Add(Error.Warning("VAL001", "Name is required"));

    if (string.IsNullOrEmpty(request.Email))
        errors.Add(Error.Warning("VAL002", "Email is required"));
    else if (!request.Email.Contains("@"))
        errors.Add(Error.Warning("VAL003", "Email format is invalid"));

    if (request.Age < 18)
        errors.Add(Error.Warning("VAL004", "Age must be 18 or older"));

    // Combine all errors into one
    if (errors.Count > 0)
        return Result.Failure<User>(Error.Multiple(errors));

    return Result.Success(new User(request.Name, request.Email, request.Age));
}
```

#### Using Error.Composite() - Advanced Approach

```csharp
public Result<User> ValidateUserAdvanced(CreateUserRequest request)
{
    var errors = new List<Error>();

    // ... collect errors as above ...

    if (errors.Count > 0)
    {
        return Result.Failure<User>(Error.Composite(
            code: "USER_VALIDATION_FAILED",
            message: "User validation failed with multiple errors",
            errors: errors,
            advice: "Please fix all validation errors and try again"));
    }

    return Result.Success(new User(request.Name, request.Email, request.Age));
}
```

### Examining Error Details

```csharp
Error compositeError = Error.Multiple(errors);

// Check if error has nested errors
if (compositeError.HasNestedErrors)
{
    Console.WriteLine($"This error contains {compositeError.NestedErrors.Count} nested errors");
    Console.WriteLine($"Total error count (recursive): {compositeError.TotalErrorCount}");
}

// Get all errors flattened
foreach (var error in compositeError.GetAllErrors())
{
    Console.WriteLine($"- {error.Code}: {error.Message}");
}
```

### Error Properties

```csharp
Error error = Error.Warning("TEST001", "Test message", "Test advice");

string code = error.Code;                    // "TEST001"
string message = error.Message;              // "Test message"
string advice = error.Advice;                // "Test advice"
Exception? exception = error.Exception;      // null (no underlying exception)
IReadOnlyList<Error> nested = error.NestedErrors; // Empty collection
bool hasNested = error.HasNestedErrors;      // false
int totalCount = error.TotalErrorCount;      // 1

// String representations
string basic = error.ToString();             // "TEST001: Test message (Advice: Test advice)"
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

## Async Support

All major extension methods have async variants that work with `Task<Result<T>>` and `Task<Result>`. These methods use `ConfigureAwait(false)` for optimal performance in library scenarios.

```csharp
// Async method chaining
Result<User> result = await GetUserIdAsync()
    .BindAsync(async id => await GetUserAsync(id))
    .MapAsync(async user => await EnrichUserAsync(user))
    .OnSuccessAsync(async user => await LogUserAccessAsync(user))
    .OnFailureAsync(async error => await NotifyAdministratorAsync(error));

// Mixed sync and async operations
Result<ProcessedData> result = await GetRawDataAsync()
    .Map(data => ValidateData(data))           // Sync validation
    .BindAsync(async data => await ProcessAsync(data))  // Async processing
    .Map(data => FormatData(data));           // Sync formatting

// Unwrap async results
int value = await GetNumberAsync().UnwrapAsync();
string name = await GetNameAsync().UnwrapOrAsync("Unknown");
```

### ToResult - Wrap Values

Convert regular values to Results. This extension method provides an explicit way to wrap values in Result types for chaining operations.

```csharp
// Wrap a value explicitly
int number = 42;
Result<int> result = number.ToResult(); // Success(42)

// Useful in method chains when you need to convert a value to Result
Result<string> processed = GetData()
    .ToResult()
    .Map(data => ProcessData(data));
```

**When to use**: When you need to explicitly convert a regular value to a Result for chaining operations.

### Unwrap - Extract Values (Use with Caution)

Extract the value from a Result, throwing an exception if it's a failure.

```csharp
// Only use when you're certain the result is successful
Result<int> result = Result.Success(42);
int value = result.Unwrap(); // Returns 42

// For void Results (just validation)
Result voidResult = Result.Success();
voidResult.Unwrap(); // Does not throw

// This will throw an InvalidOperationException
Result<int> failure = Result.Failure<int>(Error.Create("ERROR", "Something went wrong"));
int value = failure.Unwrap(); // Throws exception

// Async versions for Task<Result<T>>
Task<Result<int>> resultTask = GetDataAsync();
int value = await resultTask.UnwrapAsync(); // Returns value or throws

Task<Result> voidResultTask = ProcessAsync();
await voidResultTask.UnwrapAsync(); // Validates success or throws
```

### UnwrapOr - Extract Values with Default

Extract the value from a Result, returning a default value if it's a failure.

```csharp
// Provide a default value for failures
Result<int> result = GetNumber();
int value = result.UnwrapOr(0); // Returns actual value or 0 if failed

// Useful for optional data
Result<string> nameResult = GetUserName(id);
string displayName = nameResult.UnwrapOr("Anonymous");

// Async version
Task<Result<int>> resultTask = GetNumberAsync();
int value = await resultTask.UnwrapOrAsync(0); // Returns value or default

// Complex default values
Result<User> userResult = GetUser(id);
User user = userResult.UnwrapOr(new User { Name = "Guest", Id = -1 });
```

**When to use Unwrap**: Rarely. Only when you're absolutely certain the Result is successful. Prefer `Match` or checking `IsSuccess`.

**When to use**: When you have a sensible default value and want to continue processing even if the operation failed.

### Do - Execute Side Effects for Both Success and Failure

Use `Do` when you want to perform actions (like logging, cleanup, monitoring) that should execute regardless of whether the Result is successful or failed.

```csharp
// Simple side effect - always executes
Result<User> result = GetUser(id)
    .Do(() => Console.WriteLine("GetUser operation completed"));

// Logging both success and failure cases
Result<Order> orderResult = CreateOrder(orderData)
    .Do(() => logger.LogInformation("Order creation attempt completed"))
    .OnSuccess(order => logger.LogInformation($"Order {order.Id} created successfully"))
    .OnFailure(error => logger.LogError($"Order creation failed: {error.Message}"));

// Cleanup operations that must always run
Result<FileData> fileResult = ProcessFile(filePath)
    .Do(() => CleanupTempFiles())  // Always cleanup, regardless of success/failure
    .Do(() => UpdateMetrics());   // Always update metrics

// Multiple Do operations can be chained
Result<ProcessedData> result = ProcessData(rawData)
    .Do(() => performance.MarkStart())
    .Map(data => TransformData(data))
    .Do(() => performance.MarkEnd())
    .Do(() => auditLogger.LogOperation("DataProcessing"));

// Async version - useful for async side effects
Result<Payment> payment = await ProcessPayment(paymentData)
    .DoAsync(async () => await UpdatePaymentMetrics())
    .DoAsync(async () => await NotifyPaymentGateway());

// Practical example: Database transaction with cleanup
Result<Order> result = await BeginTransaction()
    .BindAsync(async tx => await CreateOrderInTransaction(orderData, tx))
    .Do(() => CommitOrRollbackTransaction())  // Always handle transaction
    .Do(() => CloseConnection())              // Always close connection
    .OnSuccess(order => logger.LogInformation($"Order {order.Id} committed"))
    .OnFailure(error => logger.LogError($"Transaction rolled back: {error.Message}"));
```

**When to use `Do`**:
- **Always-execute operations**: Cleanup, resource disposal, metrics collection
- **Audit logging**: Recording that an operation was attempted (regardless of outcome)
- **Performance monitoring**: Start/stop timers, profiling markers
- **Resource management**: Closing connections, files, disposing objects
- **Side effects that must happen**: Notifications, cache invalidation, etc.

**Key characteristics**:
- **Always executes**: Runs for both success and failure cases
- **Non-modifying**: Returns the original Result unchanged
- **Chainable**: Can be used multiple times in a chain
- **Exception propagation**: Any exceptions thrown in the action will be propagated

**Comparison with other extension methods**:
- `OnSuccess`: Only executes on success
- `OnFailure`: Only executes on failure
- `Do`: Always executes (both success and failure)
- `Map`: Transforms the value (only on success)
- `Bind`: Chains operations that can fail (only on success)

## Practical Examples

### Service Layer Pattern

```csharp
public class UserService
{
    public async Task<Result<User>> CreateUserAsync(CreateUserRequest request)
    {
        return await Result.Success(request)
            .Do(() => logger.LogInformation($"Starting user creation for email: {request.Email}"))
            .Bind(req => ValidateRequest(req))
            .BindAsync(async req => await CheckEmailUniqueness(req.Email))
            .BindAsync(async req => await HashPassword(req.Password))
            .BindAsync(async req => await SaveUserAsync(req))
            .Do(() => auditLogger.LogUserCreationAttempt(request.Email))  // Always log attempt
            .OnSuccessAsync(async user => await SendWelcomeEmailAsync(user))
            .OnFailure(error => logger.LogError($"User creation failed: {error.Message}"))
            .Do(() => performance.RecordUserCreationMetrics());  // Always record metrics
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
        .Do(() => performanceCounter.StartTimer("DataProcessing"))
        .Bind(d => ValidateData(d))
        .BindAsync(async d => await EnrichData(d))
        .Do(() => logger.LogInformation("Data enrichment phase completed"))
        .Map(d => TransformData(d))
        .BindAsync(async d => await SaveProcessedData(d))
        .Do(() => performanceCounter.StopTimer("DataProcessing"))  // Always stop timer
        .Do(() => CleanupTempResources())  // Always cleanup
        .OnSuccess(d => logger.LogInformation($"Processed {d.RecordCount} records successfully"))
        .OnFailure(error => logger.LogError($"Pipeline failed at: {error.Code}"));
}
```

### Configuration and Setup Pattern

```csharp
public class ApplicationSetup
{
    public async Task<int> InitializeApplicationAsync()
    {
        try
        {
            // Using UnwrapAsync for critical startup operations that must succeed
            var config = await LoadConfigurationAsync().UnwrapAsync();
            var database = await ConnectToDatabaseAsync(config.ConnectionString).UnwrapAsync();
            await RunMigrationsAsync(database).UnwrapAsync();

            // Using UnwrapOrAsync for optional features
            var cacheSize = await LoadCacheConfigurationAsync().UnwrapOrAsync(100);
            var logLevel = await LoadLogLevelAsync().UnwrapOrAsync(LogLevel.Information);

            logger.LogInformation("Application initialized successfully");
            return 0; // Success exit code
        }
        catch (InvalidOperationException ex)
        {
            logger.LogCritical($"Failed to initialize application: {ex.Message}");
            return 1; // Error exit code
        }
    }
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

// ❌ Don't do this - trying to use implicit conversions (no longer supported)
Result<string> result = "Hello World"; // Compilation error
string value = Result.Success("Hello"); // Compilation error

// ✅ Do this instead - use explicit methods
Result<string> result = Result.Success("Hello World");
string value = Result.Success("Hello").Unwrap(); // Or better: use Match or UnwrapOr
```

