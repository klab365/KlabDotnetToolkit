using System.Threading.Tasks;

namespace Klab.Toolkit.Results;


/// <summary>
/// Interface for Error
/// </summary>
public interface IError
{
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

    /// <summary>
    /// Check if the error is pending
    /// </summary>
    /// <returns></returns>
    Task<bool> IsPendingAsyc();
}
