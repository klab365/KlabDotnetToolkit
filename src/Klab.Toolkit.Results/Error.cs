using System;
using System.Collections.Generic;
using System.Linq;

namespace Klab.Toolkit.Results;

/// <summary>
/// Represents an error that occurred during an operation, containing detailed information
/// about the error including code, message, and optional exception details.
/// </summary>
public record Error
{
    /// <summary>
    /// Gets the unique error code that identifies the type of error.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the human-readable error message describing what went wrong.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets advice or suggestions on how to resolve the error, if available.
    /// </summary>
    public string Advice { get; }

    /// <summary>
    /// Gets the underlying exception that caused this error, if any.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Gets a collection of nested errors, if any.
    /// </summary>
    public IReadOnlyList<Error> NestedErrors { get; }

    /// <summary>
    /// Gets a static instance representing no error (success state).
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty, string.Empty, null, []);

    /// <summary>
    /// Creates a new Error with the specified details.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="advice">Optional advice on how to resolve the error.</param>
    /// <returns>A new Error instance.</returns>
    /// <example>
    /// <code>
    /// var error = Error.Create("VAL001", "Invalid input", "Please provide a valid email address");
    /// </code>
    /// </example>
    public static Error Create(string code, string message, string advice = "")
    {
        return new Error(code, message, advice, null);
    }

    /// <summary>
    /// Creates an error from an exception, with improved flexibility and defaults.
    /// </summary>
    /// <param name="exception">The exception to create an error from.</param>
    /// <param name="code">Optional error code. If not provided, uses the exception type name.</param>
    /// <param name="advice">Optional advice on how to resolve the error.</param>
    /// <returns>A new Error instance based on the exception.</returns>
    /// <example>
    /// <code>
    /// try { /* some operation */ }
    /// catch (ArgumentException ex)
    /// {
    ///     var error = Error.FromException(ex, "ARG001", "Check your input parameters");
    /// }
    /// </code>
    /// </example>
    public static Error FromException(Exception exception, string? code = null, string advice = "")
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        return new Error(
            code: code ?? exception.GetType().Name,
            message: exception.Message,
            advice: advice,
            exception: exception);
    }

    /// <summary>
    /// Returns a string representation of the error for debugging purposes.
    /// </summary>
    /// <returns>A formatted string containing error details.</returns>
    public override string ToString()
    {
        string result = $"{Code}: {Message}";
        if (!string.IsNullOrEmpty(Advice))
        {
            result += $" (Advice: {Advice})";
        }

        if (HasNestedErrors)
        {
            result += $" ({NestedErrors.Count} nested errors)";
        }

        return result;
    }

    /// <summary>
    /// Creates a composite error from multiple errors with a summary message.
    /// </summary>
    /// <param name="code">The unique error code for the composite error.</param>
    /// <param name="message">A summary message describing the overall failure.</param>
    /// <param name="errors">Collection of individual errors.</param>
    /// <param name="advice">Optional advice on how to resolve the errors.</param>
    /// <returns>A new Error instance containing all the nested errors.</returns>
    /// <example>
    /// <code>
    /// var validationErrors = new List&lt;Error&gt;
    /// {
    ///     Error.Warning("VAL001", "Name is required"),
    ///     Error.Warning("VAL002", "Email format is invalid")
    /// };
    /// var compositeError = Error.Composite("VALIDATION_FAILED", "Multiple validation errors occurred", validationErrors);
    /// </code>
    /// </example>
    public static Error Composite(string code, string message, IEnumerable<Error> errors, string advice = "")
    {
        if (errors == null)
        {
            throw new ArgumentNullException(nameof(errors));
        }

        List<Error> errorList = errors.ToList();
        if (errorList.Count == 0)
        {
            throw new ArgumentException("At least one error must be provided", nameof(errors));
        }

        return new Error(code, message, advice, null, errorList);
    }

    /// <summary>
    /// Creates a composite error from multiple errors with a default message.
    /// </summary>
    /// <param name="errors">Collection of individual errors.</param>
    /// <param name="code">Optional error code - defaults to "MULTIPLE_ERRORS".</param>
    /// <returns>A new Error instance containing all the nested errors.</returns>
    public static Error Multiple(IEnumerable<Error> errors, string code = "MULTIPLE_ERRORS")
    {
        if (errors == null)
        {
            throw new ArgumentNullException(nameof(errors));
        }

        List<Error> errorList = errors.ToList();
        if (errorList.Count == 0)
        {
            throw new ArgumentException("At least one error must be provided", nameof(errors));
        }

        if (errorList.Count == 1)
        {
            return errorList[0]; // Return single error directly
        }

        string message = $"Multiple errors occurred ({errorList.Count} errors)";
        string advice = "Review all nested errors for details";
        return Composite(code, message, errorList, advice);
    }

    /// <summary>
    /// Gets a value indicating whether this error has nested errors.
    /// </summary>
    public bool HasNestedErrors => NestedErrors.Count > 0;

    /// <summary>
    /// Gets the total count of all errors including nested ones (recursive).
    /// </summary>
    public int TotalErrorCount => 1 + NestedErrors.Sum(e => e.TotalErrorCount);

    /// <summary>
    /// Gets all errors flattened into a single enumerable (including this error and all nested errors recursively).
    /// </summary>
    public IEnumerable<Error> GetAllErrors()
    {
        yield return this;
        foreach (Error nestedError in NestedErrors)
        {
            foreach (Error flattenedError in nestedError.GetAllErrors())
            {
                yield return flattenedError;
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the Error record with validation.
    /// </summary>
    /// <param name="code">The unique error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="advice">Advice on how to resolve the error.</param>
    /// <param name="exception">The underlying exception, if any.</param>
    /// <param name="nestedErrors">Collection of nested errors, if any.</param>
    /// <exception cref="ArgumentNullException">Thrown when code or message is null.</exception>
    protected Error(string code, string message, string advice, Exception? exception, IReadOnlyList<Error>? nestedErrors = null)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Advice = advice ?? string.Empty;
        Exception = exception;
        NestedErrors = nestedErrors ?? Array.Empty<Error>();
    }
}
