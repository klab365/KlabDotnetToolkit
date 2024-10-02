using System;
using System.Threading.Tasks;

namespace Klab.Toolkit.Results;

/// <summary>
/// Informative Error which is not pending, if the error was read by the user!!
/// </summary>
public record InformativeError : IError
{
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
    /// Create a new Error
    /// </summary>
    /// <param name="code"></param>
    /// <param name="message"></param>
    /// <param name="advice"></param>
    public InformativeError(string code, string message, string advice = "")
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
    public static InformativeError FromException(string id, Exception ex)
    {
        return new InformativeError(id, ex.Message, ex.StackTrace ?? string.Empty);
    }

    /// <inheritdoc/>
    public Task<bool> IsPendingAsyc()
    {
        return Task.FromResult(false);
    }
}
