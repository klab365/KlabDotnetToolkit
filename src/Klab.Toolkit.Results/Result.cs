using System;

namespace Klab.Toolkit.Results;

/// <summary>
/// Represents a result of an operation
/// Additionally it contains a value which can be used to return the result of the operation
/// and error messages if the operation failed.
/// </summary>
public record Result : IResult
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
    /// Generate a success with a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Result<T> Success<T>(T value) where T : notnull
    {
        return new Result<T>(value, true, new ErrorNone());
    }

    /// <summary>
    /// Generate a failure.
    /// </summary>
    public static Result Failure(IError error)
    {
        return new Result(false, error);
    }

    /// <summary>
    /// Generate a failure with a value (default).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result<T> Failure<T>(IError error) where T : notnull
    {
        return new Result<T>(default!, false, error);
    }
}


/// <summary>
/// Generic Result
/// </summary>
/// <typeparam name="T"></typeparam>
public record Result<T> : Result, IResult<T> where T : notnull
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
    internal Result(T value, bool isSuccess, IError error) : base(isSuccess, error)
    {
        _value = value;
    }
}
