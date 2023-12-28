using System;

namespace Klab.Toolkit.Results;

/// <summary>
/// Error class
/// </summary>
public record Error
{
    /// <summary>
    /// None Error
    /// </summary>
    public static Error None => new(0, string.Empty);

    /// <summary>
    /// Error code
    /// </summary>
    public int Code { get; }

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
    public string StackTrace { get; set; } = string.Empty;

    /// <summary>
    /// Create a new Error
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="advice"></param>
    public Error(int code, string message, string advice = "")
    {
        Code = code;
        Message = message;
        Advice = advice;
    }

    /// <summary>
    /// Generate a implicit error from an exception
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ex"></param>
    public static Error FromException(int id, Exception ex)
    {
        return new Error(id, ex.Message, ex.StackTrace ?? string.Empty);
    }
}
