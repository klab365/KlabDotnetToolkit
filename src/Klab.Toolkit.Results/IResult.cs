namespace Klab.Toolkit.Results;

/// <summary>
/// Interface for a result of an operation
/// </summary>
public interface IResult
{
    /// <summary>
    /// Gets a value indicating whether indicates the operation was successful.
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Flag to indicate the operation failed.
    /// </summary>
    bool IsFailure { get; }

    /// <summary>
    /// Gets contains Errors.
    /// </summary>
    IError Error { get; }
}

/// <summary>
/// Interface for a result of an operation with a value
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IResult<T> : IResult where T : notnull
{
    /// <summary>
    /// Gets the value of the result
    /// </summary>
    T Value { get; }
}

