namespace Klab.Toolkit.Results;

/// <summary>
/// Exposes a successful result value without runtime reflection.
/// </summary>
public interface IResultWithValue
{
    /// <summary>
    /// Gets whether the operation succeeded.
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets the result value when the operation succeeded.
    /// </summary>
    object? GetValue();
}
