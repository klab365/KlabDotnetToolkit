using System;

namespace Klab.Toolkit.Results;

/// <summary>
/// Represents a result of an operation
/// Additionally it contains a value which can be used to return the result of the operation
/// and error messages if the operation failed.
/// </summary>
public record Result
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
        return new Result(true, Results.Error.None());
    }

    /// <summary>
    /// Generate a success with a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Result<T> Success<T>(T value) where T : notnull
    {
        return new Result<T>(value, true, Error.None());
    }

    /// <summary>
    /// Generate a failure.
    /// </summary>
    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }

    /// <summary>
    /// Generate a failure with a value (default).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result<T> Failure<T>(Error error) where T : notnull
    {
        return new Result<T>(default!, false, error);
    }

    /// <summary>
    /// Implicit conversion from Result to bool
    /// </summary>
    /// <param name="result"></param>
    public static implicit operator bool(Result result) => result.IsSuccess;

    /// <summary>
    /// Implicit conversion error to Result
    /// </summary>
    public static implicit operator Result(Error error) => Failure(error);
}


/// <summary>
/// Generic Result
/// </summary>
/// <typeparam name="T"></typeparam>
public record Result<T> : Result where T : notnull
{
    private readonly T _value;

    /// <summary>
    /// Gets value which contains the result of the operation.
    /// </summary>
    public T Value => IsSuccess ? _value : throw new InvalidOperationException($"Cannot access value: operation failed with error: {Error}");

    /// <summary>
    /// Implicit conversion from T to Result.
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Implicit conversion from Error to Result (failure case)
    /// </summary>
    /// <param name="error"></param>
    public static implicit operator Result<T>(Error error) => Failure<T>(error);

    /// <summary>
    /// Implicit conversion from Result to bool
    /// </summary>
    /// <param name="result"></param>
    public static implicit operator bool(Result<T> result) => result.IsSuccess;

    /// <summary>
    /// Implicit conversion from Result to T
    /// </summary>
    /// <param name="result"></param>
    public static implicit operator T(Result<T> result) => result.Value;

    /// <summary>
    /// Protected constructor
    /// </summary>
    internal Result(T value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }
}
