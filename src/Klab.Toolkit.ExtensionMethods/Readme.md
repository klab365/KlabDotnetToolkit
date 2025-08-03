# Klab.Toolkit.ExtensionMethods

## Overview

The `Klab.Toolkit.ExtensionMethods` package provides a comprehensive collection of extension methods that enhance built-in .NET types with frequently needed functionality. These extensions promote cleaner, more readable code while reducing boilerplate and improving developer productivity.

## Purpose

This package extends common .NET types with practical functionality:

- **String Operations**: Case conversions, validation, formatting, and parsing
- **Enum Utilities**: Description handling, reflection-based operations, and metadata extraction
- **Type Inspection**: Generic type analysis, interface checking, and reflection helpers
- **Validation**: Built-in validation for common patterns (email, phone, etc.)

## Key Features

### String Extensions
- Case conversion methods (PascalCase, camelCase, kebab-case, etc.)
- Validation methods for emails, phone numbers, URLs
- HTML manipulation and sanitization
- String parsing and formatting utilities

### Enum Extensions
- Description attribute extraction
- Enum-to-dictionary conversions
- Metadata operations for enum types

### Type Extensions
- Generic type analysis
- Interface compatibility checking
- Reflection utilities for type inspection

## Installation

```bash
dotnet add package Klab.Toolkit.ExtensionMethods
```

## String Extensions

### String Validation

```csharp
// Email validation
string email = "user@example.com";
bool isValidEmail = email.IsValidEmail(); // true

string invalidEmail = "not-an-email";
bool isInvalid = invalidEmail.IsValidEmail(); // false

// Phone number validation
string phone = "+1 (555) 123-4567";
bool isValidPhone = phone.IsValidPhoneNumber(); // true

// URL validation
string url = "https://www.example.com";
bool isValidUrl = url.IsValidUrl(); // true

// Check if string contains only letters
string letters = "HelloWorld";
bool onlyLetters = letters.ContainsOnlyLetters(); // true

string mixed = "Hello123";
bool mixedCheck = mixed.ContainsOnlyLetters(); // false
```

### HTML and Content Manipulation

```csharp
// Remove HTML tags
string htmlContent = "<p>Hello <strong>World</strong>!</p>";
string plainText = htmlContent.RemoveHtmlTags(); // "Hello World!"

// Remove HTML comments
string htmlWithComments = "<p>Content</p><!-- This is a comment -->";
string cleaned = htmlWithComments.RemoveHtmlComments(); // "<p>Content</p>"

// Remove scripts and styles
string htmlWithScript = "<p>Content</p><script>alert('xss')</script>";
string safe = htmlWithScript.RemoveHtmlScripts(); // "<p>Content</p>"

// URL decoding
string encoded = "Hello%20World%21";
string decoded = encoded.UrlDecode(); // "Hello World!"
```

### String Utilities

```csharp
// Null/whitespace checking
string empty = "";
string whitespace = "   ";
string content = "Hello";

bool isEmpty1 = empty.IsNullOrWhiteSpace(); // true
bool isEmpty2 = whitespace.IsNullOrWhiteSpace(); // true
bool isEmpty3 = content.IsNullOrWhiteSpace(); // false

// Truncation with ellipsis
string longText = "This is a very long text that needs to be truncated";
string truncated = longText.Truncate(20); // "This is a very long..."

// Safe substring
string text = "Hello World";
string safe1 = text.SafeSubstring(0, 5); // "Hello"
string safe2 = text.SafeSubstring(0, 100); // "Hello World" (no exception)

// Reverse string
string original = "Hello";
string reversed = original.Reverse(); // "olleH"
```

### Practical Examples

```csharp
public class UserRegistrationService
{
    public Result<User> ValidateAndCreateUser(string email, string phone, string name)
    {
        // Validate email
        if (!email.IsValidEmail())
        {
            return Error.Create("INVALID_EMAIL", "Please provide a valid email address");
        }

        // Validate phone (optional)
        if (!string.IsNullOrWhiteSpace(phone) && !phone.IsValidPhoneNumber())
        {
            return Error.Create("INVALID_PHONE", "Please provide a valid phone number");
        }

        // Clean and format name
        var cleanName = name.RemoveHtmlTags().Trim();
        if (cleanName.IsNullOrWhiteSpace())
        {
            return Error.Create("INVALID_NAME", "Name is required");
        }

        var user = new User
        {
            Email = email.ToLowerInvariant(),
            Phone = phone,
            DisplayName = cleanName.ToTitleCase(),
            Username = cleanName.ToCamelCase()
        };

        return Result.Success(user);
    }
}

public class ContentService
{
    public string SanitizeUserContent(string userInput)
    {
        return userInput
            .RemoveHtmlTags()
            .RemoveHtmlComments()
            .RemoveHtmlScripts()
            .Trim();
    }

    public string CreateSlug(string title)
    {
        return title
            .ToLowerInvariant()
            .RemoveHtmlTags()
            .ToKebabCase()
            .Truncate(50);
    }
}
```

## Enum Extensions

### Description Attribute Handling

```csharp
using System.ComponentModel;

// Define enum with descriptions
public enum OrderStatus
{
    [Description("Order is pending approval")]
    Pending,

    [Description("Order has been approved and is being processed")]
    Processing,

    [Description("Order has been shipped to customer")]
    Shipped,

    [Description("Order has been delivered successfully")]
    Delivered,

    [Description("Order has been cancelled")]
    Cancelled
}

// Usage
OrderStatus status = OrderStatus.Processing;
string description = status.GetDescription();
// "Order has been approved and is being processed"

// Without description attribute
OrderStatus pending = OrderStatus.Pending;
string defaultDesc = pending.GetDescription(); // "Pending"
```

### Enum Metadata Operations

```csharp
// Get all enum values with descriptions as dictionary
Dictionary<string, string> statusDescriptions =
    EnumExtensions.GetDictionaryWithEnumNameAndDescription<OrderStatus>();

// Results in:
// {
//   "Pending": "Order is pending approval",
//   "Processing": "Order has been approved and is being processed",
//   "Shipped": "Order has been shipped to customer",
//   "Delivered": "Order has been delivered successfully",
//   "Cancelled": "Order has been cancelled"
// }

// Use in dropdowns or UI
public class OrderController : ControllerBase
{
    [HttpGet("statuses")]
    public IActionResult GetOrderStatuses()
    {
        var statuses = EnumExtensions.GetDictionaryWithEnumNameAndDescription<OrderStatus>()
            .Select(kvp => new { Value = kvp.Key, Text = kvp.Value })
            .ToList();

        return Ok(statuses);
    }
}
```

### Advanced Enum Usage

```csharp
public enum Priority
{
    [Description("Low priority - can be handled later")]
    Low = 1,

    [Description("Normal priority - standard processing")]
    Normal = 2,

    [Description("High priority - expedited processing")]
    High = 3,

    [Description("Critical priority - immediate attention required")]
    Critical = 4
}

public class TaskService
{
    public string GetPriorityDisplayText(Priority priority)
    {
        return priority.GetDescription();
    }

    public Dictionary<string, string> GetAllPriorities()
    {
        return EnumExtensions.GetDictionaryWithEnumNameAndDescription<Priority>();
    }

    public IEnumerable<SelectListItem> GetPrioritySelectItems()
    {
        return EnumExtensions.GetDictionaryWithEnumNameAndDescription<Priority>()
            .Select(kvp => new SelectListItem
            {
                Value = kvp.Key,
                Text = kvp.Value
            });
    }
}

// Usage in validation
public class CreateTaskRequest
{
    public string Title { get; set; }
    public Priority Priority { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.IsDefined(typeof(Priority), Priority))
        {
            yield return new ValidationResult(
                $"Invalid priority. Valid values are: {string.Join(", ", EnumExtensions.GetDictionaryWithEnumNameAndDescription<Priority>().Keys)}",
                new[] { nameof(Priority) }
            );
        }
    }
}
```

## Type Extensions

### Generic Type Analysis

```csharp
using Klab.Toolkit.Common.Extensions;

// Check if type can be cast to another
Type stringType = typeof(string);
Type objectType = typeof(object);
bool canCast = stringType.CanBeCastTo(objectType); // true

// Generic type compatibility
Type openGeneric = typeof(List<>);
Type closedGeneric = typeof(List<string>);
bool couldClose = openGeneric.CouldCloseTo(closedGeneric); // true

// Find interfaces that close to a template
Type concreteType = typeof(List<string>);
Type templateInterface = typeof(IEnumerable<>);
var interfaces = concreteType.FindInterfacesThatClose(templateInterface);
// Returns interfaces like IEnumerable<string>
```

### Practical Type Inspection

```csharp
public class ServiceRegistrationHelper
{
    public static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerInterface = typeof(IRequestHandler<,>);

        var handlerTypes = assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => type.GetInterfaces()
                .Any(i => i.IsGenericType &&
                         i.GetGenericTypeDefinition().CanBeCastTo(handlerInterface)))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var interfaces = handlerType.FindInterfacesThatClose(handlerInterface);
            foreach (var interfaceType in interfaces)
            {
                services.AddTransient(interfaceType, handlerType);
            }
        }
    }
}

public class TypeValidator
{
    public static bool IsValidServiceType(Type type)
    {
        // Check if type can be cast to required interface
        return type.CanBeCastTo(typeof(IDisposable)) ||
               type.GetInterfaces().Any(i => i.Name.EndsWith("Service"));
    }

    public static IEnumerable<Type> FindImplementations<T>(Assembly assembly)
    {
        var targetType = typeof(T);
        return assembly.GetTypes()
            .Where(type => type.IsClass &&
                          !type.IsAbstract &&
                          type.CanBeCastTo(targetType));
    }
}
```

## Real-World Integration Examples

### API Response Formatting

```csharp
public class ApiResponseService
{
    public object FormatErrorResponse(string errorCode, string errorMessage)
    {
        return new
        {
            Error = new
            {
                Code = errorCode.ToUpperInvariant(),
                Message = errorMessage.RemoveHtmlTags(),
                Timestamp = DateTime.UtcNow
            }
        };
    }

    public string GenerateSlugFromTitle(string title)
    {
        return title
            .RemoveHtmlTags()
            .ToLowerInvariant()
            .ToKebabCase()
            .Truncate(50);
    }
}
```

### Configuration Processing

```csharp
public class ConfigurationProcessor
{
    public Dictionary<string, object> ProcessEnvironmentVariables()
    {
        var config = new Dictionary<string, object>();

        foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
        {
            var key = env.Key.ToString();
            var value = env.Value?.ToString() ?? "";

            // Convert environment variable names to camelCase
            var camelKey = key.ToLowerInvariant().ToCamelCase();

            // Clean and validate values
            if (!value.IsNullOrWhiteSpace())
            {
                // Check if it's an email
                if (value.IsValidEmail())
                {
                    config[camelKey] = value.ToLowerInvariant();
                }
                // Check if it's a URL
                else if (value.IsValidUrl())
                {
                    config[camelKey] = value;
                }
                // Regular string value
                else
                {
                    config[camelKey] = value.RemoveHtmlTags();
                }
            }
        }

        return config;
    }
}
```

### Data Import/Export

```csharp
public class DataExportService
{
    public string ExportEnumToJson<T>() where T : Enum
    {
        var enumData = EnumExtensions.GetDictionaryWithEnumNameAndDescription<T>();
        return JsonSerializer.Serialize(enumData, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    public string SanitizeForCsv(string input)
    {
        return input
            .RemoveHtmlTags()
            .Replace("\"", "\"\"")  // Escape quotes for CSV
            .Replace("\n", " ")     // Remove line breaks
            .Replace("\r", " ")
            .Trim();
    }

    public string CreateFilename(string baseName)
    {
        return baseName
            .RemoveHtmlTags()
            .ToKebabCase()
            .Truncate(50) +
            $"-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
    }
}
```

## Performance Considerations

- **String Operations**: Most string extensions use efficient algorithms and built-in .NET methods
- **Regex Caching**: Validation methods use compiled and cached regex patterns
- **Type Operations**: Reflection-based operations are optimized but should be used judiciously in hot paths
- **Memory Allocation**: Extension methods minimize unnecessary string allocations where possible

## Thread Safety

All extension methods in this package are thread-safe:
- String extensions are stateless and operate on immutable strings
- Enum extensions use reflection but don't modify state
- Type extensions perform read-only operations on type metadata

## Best Practices

### String Extensions
- Use validation extensions for user input sanitization
- Prefer case conversion extensions over manual string manipulation
- Use HTML removal methods when processing user-generated content

### Enum Extensions
- Always use `[Description]` attributes for user-facing enum values
- Cache enum dictionaries in long-running applications
- Use enum extensions for generating dropdowns and select lists

### Type Extensions
- Use type compatibility checking in generic constraint scenarios
- Leverage interface finding for automatic service registration
- Cache reflection results in performance-critical applications

## Common Patterns

### Input Validation Pipeline
```csharp
public class InputValidator
{
    public Result<string> ValidateAndCleanInput(string input)
    {
        // Clean HTML
        var cleaned = input.RemoveHtmlTags().RemoveHtmlComments();

        // Validate not empty
        if (cleaned.IsNullOrWhiteSpace())
            return Error.Create("EMPTY_INPUT", "Input cannot be empty");

        // Validate length
        if (cleaned.Length > 1000)
            return Error.Create("INPUT_TOO_LONG", "Input exceeds maximum length");

        return Result.Success(cleaned.Trim());
    }
}
```

### API Slug Generation
```csharp
public class SlugGenerator
{
    public string GenerateSlug(string title)
    {
        return title
            .RemoveHtmlTags()
            .ToLowerInvariant()
            .ToKebabCase()
            .Truncate(50)
            .Trim('-');
    }
}
```
