namespace Klab.Toolkit.Results;

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
    /// Flag to indicate the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets contains Errors.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Generate a success.
    /// </summary>
    public static Result Success()
    {
        return new Result(true, Error.None);
    }

    /// <summary>
    /// Generate a failure.
    /// </summary>
    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }

    /// <summary>
    /// Generate a error implicit from error
    /// </summary>
    /// <param name="error"></param>
    public static implicit operator Result(Error error)
    {
        return Failure(error);
    }
}


/// <summary>
/// Generic Result
/// </summary>
/// <typeparam name="T"></typeparam>
public class Result<T> : Result
{
    /// <summary>
    /// Gets value which contains the result of the operation.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Protected constructor
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isSuccess"></param>
    /// <param name="error"></param>
    protected Result(T? value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        Value = value;
    }

    /// <summary>
    /// Generate a implicit success from a value
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Result<T>(T? value)
    {
        return new Result<T>(value, true, Error.None);
    }

    /// <summary>
    /// Create implicit failure from an error
    /// </summary>
    /// <param name="error"></param>
    public static implicit operator Result<T>(Error error)
    {
        return new Result<T>(default, false, error);
    }
}
