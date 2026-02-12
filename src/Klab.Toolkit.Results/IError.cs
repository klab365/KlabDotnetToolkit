using System;
using System.Collections.Generic;

namespace Klab.Toolkit.Results;

/// <summary>
/// Defines the contract for error information in result types.
/// Implement this interface to create domain-specific error types.
/// </summary>
public interface IError
{
    /// <summary>
    /// Gets the unique error code that identifies the type of error.
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Gets the human-readable error message describing what went wrong.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Gets advice or suggestions on how to resolve the error, if available.
    /// </summary>
    string Advice { get; }

    /// <summary>
    /// Gets the underlying exception that caused this error, if any.
    /// </summary>
    Exception? Exception { get; }

    /// <summary>
    /// Gets a collection of nested errors, if any.
    /// </summary>
    IReadOnlyList<IError> NestedErrors { get; }

    /// <summary>
    /// Gets a value indicating whether this error has nested errors.
    /// </summary>
    bool HasNestedErrors { get; }

    /// <summary>
    /// Gets the total count of all errors including nested ones (recursive).
    /// </summary>
    int TotalErrorCount { get; }

    /// <summary>
    /// Gets all errors flattened into a single enumerable (including this error and all nested errors recursively).
    /// </summary>
    IEnumerable<IError> GetAllErrors();
}
