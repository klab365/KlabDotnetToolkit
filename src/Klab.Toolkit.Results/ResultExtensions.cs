using System;

namespace Klab.Toolkit.Results;

/// <summary>
/// Extensions for the Result class.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Map transforms the success value if the result is successful
    /// </summary>
    public static Result<TResult> Map<TSource, TResult>(this Result<TSource> result, Func<TSource, TResult> mapFunc) where TSource : notnull where TResult : notnull
    {
        return result.IsSuccess ? Result.Success(mapFunc(result.Value)) : Result.Failure<TResult>(result.Error);
    }

    /// <summary>
    /// Bind chains operations that return Result types, allowing for error propagation
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="result"></param>
    /// <param name="bindFunc"></param>
    /// <returns></returns>
    public static Result<TResult> Bind<TSource, TResult>(this Result<TSource> result, Func<TSource, Result<TResult>> bindFunc) where TSource : notnull where TResult : notnull
    {
        return result.IsSuccess ? bindFunc(result.Value) : Result.Failure<TResult>(result.Error);
    }

    /// <summary>
    /// Tap executes an action when the result is successful
    /// </summary>
    /// <param name="result"></param>
    /// <param name="successAction"></param>
    /// <returns></returns>
    public static Result Tap(this Result result, Action<Result> successAction)
    {
        if (result.IsSuccess)
        {
            successAction(result);
        }

        return result;
    }

    /// <summary>
    /// Tap executes an action when the result is successful
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <param name="successAction"></param>
    /// <returns></returns>
    public static Result<T> Tap<T>(this Result<T> result, Action<Result<T>> successAction) where T : notnull
    {
        if (result.IsSuccess)
        {
            successAction(result);
        }

        return result;
    }

    /// <summary>
    /// OnFailure executes an action when the result is a failure
    /// </summary>
    /// <param name="result"></param>
    /// <param name="failureAction"></param>
    /// <returns></returns>
    public static Result OnFailure(this Result result, Action<Error> failureAction)
    {
        if (!result.IsSuccess)
        {
            failureAction(result.Error);
        }

        return result;
    }

    /// <summary>
    /// Ensure ensures certain conditions are met before proceeding with the operation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <param name="conditionFunc"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> conditionFunc, Error error) where T : notnull
    {
        if (!result.IsSuccess || !conditionFunc(result.Value))
        {
            return Result.Failure<T>(error);
        }

        return result;
    }

    /// <summary>
    /// ToResult converts a value to a Result type, wrapping it in a success result
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Result<T> ToResult<T>(this T value) where T : notnull
    {
        return Result.Success(value);
    }

    /// <summary>
    /// Unwrap retrieves the value from a successful result, throwing an exception if the result is a failure
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static T Unwrap<T>(this Result<T> result) where T : notnull
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw new InvalidOperationException("Cannot unwrap a failure result");
    }

    /// <summary>
    /// Executes the appropriate function based on success or failure.
    /// </summary>
    public static TResult Match<T, TResult>(
        this Result<T> result,
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure) where T : notnull
    {
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);
    }

    /// <summary>
    /// Executes the appropriate action based on success or failure without modifying the result.
    /// </summary>
    public static void Match<T>(
        this Result<T> result,
        Action<T> onSuccess,
        Action<Error> onFailure) where T : notnull
    {
        if (result.IsSuccess)
        {
            onSuccess(result.Value);
        }
        else
        {
            onFailure(result.Error);
        }
    }
}
