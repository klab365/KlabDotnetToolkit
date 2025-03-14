namespace Klab.Toolkit.Results;


/// <summary>
/// Interface for Error
/// </summary>
public interface IError
{
    /// <summary>
    /// Type of the error
    /// </summary>
    ErrorType Type { get; }

    /// <summary>
    /// Error code
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Message
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Advice of the error if any
    /// </summary>
    string Advice { get; }

    /// <summary>
    /// StackTrace (optional)
    /// </summary>
    string? StackTrace { get; }
}
