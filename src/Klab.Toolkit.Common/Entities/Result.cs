﻿namespace Klab.Toolkit.Common.Entities;

/// <summary>
/// Represents a result of an operation
/// Additionally it contains a value which can be used to return the result of the operation
/// and error messages if the operation failed.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Result<T>
    where T : struct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <param name="succeeded"></param>
    /// <param name="value"></param>
    /// <param name="errors"></param>
    internal Result(bool succeeded, T? value, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Value = value;
        Errors = errors.ToArray();
    }

    /// <summary>
    /// Gets a value indicating whether indicates the operation was successfull.
    /// </summary>
    public bool Succeeded { get; }

    /// <summary>
    /// Gets contains Errors.
    /// </summary>
    public string[] Errors { get; }

    /// <summary>
    /// Gets value which contains the result of the operation.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Generate a success.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, Array.Empty<string>());
    }

    /// <summary>
    /// Genearate a failure.
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public static Result<T> Failure(IEnumerable<string> errors)
    {
        return new Result<T>(false, null, errors);
    }
}