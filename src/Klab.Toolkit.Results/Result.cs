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
    /// Generate a success with a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Result<T> Success<T>(T value)
    {
        return new Result<T>(value, true, Error.None);
    }

    /// <summary>
    /// Creates a new <see cref="Result{TValue}"/> with the specified nullable value and the specified error.
    /// </summary>
    /// <typeparam name="TValue">The result type.</typeparam>
    /// <param name="value">The result value.</param>
    /// <param name="error">The error in case the value is null.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/> with the specified value or an error.</returns>
    public static Result<TValue> Create<TValue>(TValue value, Error error) where TValue : class
    {
        return value is null ? Failure<TValue>(error) : Success(value);
    }

    /// <summary>
    /// Generate a failure.
    /// </summary>
    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }

    /// <summary>
    /// Returns a failure <see cref="Result{TValue}"/> with the specified error.
    /// </summary>
    /// <typeparam name="TValue">The result type.</typeparam>
    /// <param name="error">The error.</param>
    /// <returns>A new instance of <see cref="Result{TValue}"/> with the specified error and failure flag set.</returns>
    /// <remarks>
    /// We're purposefully ignoring the nullable assignment here because the API will never allow it to be accessed.
    /// The value is accessed through a method that will throw an exception if the result is a failure result.
    /// </remarks>
    public static Result<TValue> Failure<TValue>(Error error)
    {
        return new Result<TValue>(default!, false, error);
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
    private readonly T _value;


    /// <summary>
    /// Gets value which contains the result of the operation.
    /// </summary>
    public T? Value => IsSuccess ? _value : throw new InvalidOperationException("No value for failure result");

    /// <summary>
    /// Protected constructor
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isSuccess"></param>
    /// <param name="error"></param>
    protected internal Result(T value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Generate a implicit success from a value
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }
}
