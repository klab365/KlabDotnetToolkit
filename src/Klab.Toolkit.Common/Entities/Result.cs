namespace Klab.Toolkit.Common.Entities;

/// <summary>
/// Represents a result of an operation
/// Additionally it contains a value which can be used to return the result of the operation
/// and error messages if the operation failed.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <param name="isSucceeded"></param>
    /// <param name="error"></param>
    protected Result(bool isSucceeded, Error error)
    {
        IsSuccess = isSucceeded;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether indicates the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Get the indication if the result is a failure.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets contains Errors.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Generate a success.
    /// </summary>
    /// <returns></returns>
    public static Result Success()
    {
        return new Result(true, Error.None);
    }

    /// <summary>
    /// Genearate a failure.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }
}


/// <summary>
/// Generic Result
/// </summary>
/// <typeparam name="T"></typeparam>
public class Result<T> : Result
{
    private readonly T? _value;

    /// <summary>
    /// Gets value which contains the result of the operation.
    /// </summary>
    public T? Value => IsSuccess ?
        _value
        : throw new InvalidOperationException("The value of a failure result cannot be accessed.");

    /// <summary>
    /// Protected constructor
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isSuccess"></param>
    /// <param name="error"></param>
    protected Result(T? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Generate a implicit success from a value
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Result<T>(T? value)
    {
        return new Result<T>(value, true, Error.None);
    }
}
