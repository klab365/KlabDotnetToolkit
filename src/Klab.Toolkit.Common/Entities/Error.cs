namespace Klab.Toolkit.Common.Entities;

/// <summary>
/// Error class
/// </summary>
public class Error
{
    /// <summary>
    /// None Error
    /// </summary>
    public static Error None => new(string.Empty, string.Empty, string.Empty);

    /// <summary>
    /// Error code
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Message
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// StackTrace
    /// </summary>
    public string StackTrace { get; }

    /// <summary>
    /// Create a new Error
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="stackTrace"></param>
    public Error(string code, string message, string stackTrace)
    {
        Code = code;
        Message = message;
        StackTrace = stackTrace;
    }

    /// <summary>
    /// Generate a implicit error from the message
    /// </summary>
    /// <param name="message"></param>
    public static implicit operator Error(string message)
    {
        return new Error(string.Empty, message, string.Empty);
    }

    /// <summary>
    /// Generate a implicit error from an exception
    /// </summary>
    /// <param name="ex"></param>
    public static implicit operator Error(Exception ex)
    {
        return new Error(string.Empty, ex.Message, ex.StackTrace ?? string.Empty);
    }
}
