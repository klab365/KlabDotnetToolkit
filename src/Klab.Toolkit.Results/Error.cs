using System;

namespace Klab.Toolkit.Results;

/// <summary>
/// Informative Error which is not pending, if the error was read by the user!!
/// </summary>
public record Error : IError
{
    /// <inheritdoc/>
    public ErrorType Type { get; }

    /// <summary>
    /// Error code
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Message
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Advice of the error if any
    /// </summary>
    public string Advice { get; }

    /// <summary>
    /// StackTrace
    /// </summary>
    public string? StackTrace { get; }

    /// <summary>
    /// None
    /// </summary>
    public static Error None() => new Error(string.Empty, string.Empty, string.Empty, ErrorType.Info, null);

    /// <summary>
    /// Create a new Error
    /// </summary>
    public static Error Create(string code, string message, string advice = "", ErrorType type = ErrorType.Info)
    {
        return new Error(code, message, advice, type, null);
    }

    /// <summary>
    /// Generate a implicit error from an exception
    /// </summary>
    public static Error FromException(string id, ErrorType errorType, Exception ex)
    {
        return new Error(
            code: id,
            message: ex.Message,
            advice: string.Empty,
            type: errorType,
            stackTrace: ex.StackTrace ?? string.Empty);
    }

    /// <summary>
    /// Generate a implicit error from an exception
    /// </summary>
    protected Error(string code, string message, string advice, ErrorType type, string? stackTrace)
    {
        Code = code;
        Message = message;
        Advice = advice;
        Type = type;
        StackTrace = stackTrace;
    }
}
