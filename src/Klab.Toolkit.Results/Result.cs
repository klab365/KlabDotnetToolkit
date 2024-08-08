using System;

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
    protected Result(bool isSucceeded, IError error)
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
    public IError Error { get; }

    /// <summary>
    /// Generate a success.
    /// </summary>
    public static Result Success()
    {
        return new Result(true, new ErrorNone());
    }

    /// <summary>
    /// Generate a failure.
    /// </summary>
    public static Result Failure(InformativeError error)
    {
        return new Result(false, error);
    }

    /// <summary>
    /// Generate a error implicit from error
    /// </summary>
    /// <param name="error"></param>
    public static implicit operator Result(InformativeError error)
    {
        return Failure(error);
    }
}


/// <summary>
/// Generic Result
/// </summary>
/// <typeparam name="T"></typeparam>
public class Result<T> : Result where T : notnull
{
    private readonly T _value;

    /// <summary>
    /// Gets value which contains the result of the operation.
    /// </summary>
    public T Value => IsSuccess ? _value : throw new InvalidOperationException("Cannot access value of failure result");

    /// <summary>
    /// Protected constructor
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isSuccess"></param>
    /// <param name="error"></param>
    protected Result(T value, bool isSuccess, IError error) : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Generate a success with a value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Result<T> Success(T value)
    {
        return new Result<T>(value, true, new ErrorNone());
    }

    /// <summary>
    /// Generate a failure with an error
    /// </summary>
    public static Result<T> CreateFailure(InformativeError error)
    {
        return new Result<T>(default!, false, error);
    }

    /// <summary>
    /// Generate a implicit success from a value
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Result<T>(T value)
    {
        return new Result<T>(value, true, new ErrorNone());
    }

    /// <summary>
    /// Create implicit failure from an error
    /// </summary>
    /// <param name="error"></param>
    public static implicit operator Result<T>(InformativeError error)
    {
        return new Result<T>(default!, false, error);
    }
}
