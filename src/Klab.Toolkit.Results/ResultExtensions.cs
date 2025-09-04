using System;
using System.Threading.Tasks;

namespace Klab.Toolkit.Results;

/// <summary>
/// Extensions for the Result class.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps the success value if the result is successful.
    /// </summary>
    public static Result<TResult> Map<TSource, TResult>(
        this Result<TSource> result,
        Func<TSource, TResult> mapFunc) where TSource : notnull where TResult : notnull
    {
        return result.IsSuccess
            ? Result.Success(mapFunc(result.Value))
            : Result.Failure<TResult>(result.Error);
    }

    /// <summary>
    /// Asynchronously maps the success value if the result is successful.
    /// </summary>
    public static async Task<Result<TResult>> MapAsync<TSource, TResult>(
        this Task<Result<TSource>> resultTask,
        Func<TSource, Task<TResult>> mapFunc) where TSource : notnull where TResult : notnull
    {
        Result<TSource> result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess
            ? Result.Success(await mapFunc(result.Value).ConfigureAwait(false))
            : Result.Failure<TResult>(result.Error);
    }

    /// <summary>
    /// Chains operations that return Result types, allowing for error propagation.
    /// </summary>
    public static Result<TResult> Bind<TSource, TResult>(
        this Result<TSource> result,
        Func<TSource, Result<TResult>> bindFunc) where TSource : notnull where TResult : notnull
    {
        return result.IsSuccess
            ? bindFunc(result.Value)
            : Result.Failure<TResult>(result.Error);
    }

    /// <summary>
    /// Asynchronously chains operations that return Result types, allowing for error propagation.
    /// </summary>
    public static async Task<Result<TResult>> BindAsync<TSource, TResult>(
        this Task<Result<TSource>> resultTask,
        Func<TSource, Task<Result<TResult>>> bindFunc) where TSource : notnull where TResult : notnull
    {
        Result<TSource> result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess
            ? await bindFunc(result.Value).ConfigureAwait(false)
            : Result.Failure<TResult>(result.Error);
    }    /// <summary>
         /// Chains operations that return Result types, allowing for error propagation.
         /// </summary>
    public static Result Bind(
        this Result result,
        Func<Result> bindFunc)
    {
        return result.IsSuccess
            ? bindFunc()
            : Result.Failure(result.Error);
    }

    /// <summary>
    /// Chains operations that return Result types, allowing for error propagation.
    /// </summary>
    public static Result<T> Bind<T>(
        this Result result,
        Func<Result<T>> bindFunc) where T : notnull
    {
        return result.IsSuccess
            ? bindFunc()
            : Result.Failure<T>(result.Error);
    }

    /// <summary>
    /// Asynchronously chains operations that return Result types, allowing for error propagation.
    /// </summary>
    public static async Task<Result> BindAsync(
        this Task<Result> resultTask,
        Func<Task<Result>> bindFunc)
    {
        Result result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess
            ? await bindFunc().ConfigureAwait(false)
            : Result.Failure(result.Error);
    }

    /// <summary>
    /// Asynchronously chains operations that return Result types, allowing for error propagation.
    /// This overload allows binding directly from Result.Success() to an async operation.
    /// </summary>
    public static async Task<Result> BindAsync(
        this Result result,
        Func<Task<Result>> bindFunc)
    {
        return result.IsSuccess
            ? await bindFunc().ConfigureAwait(false)
            : Result.Failure(result.Error);
    }

    /// <summary>
    /// Asynchronously chains operations that return Result types with values, allowing for error propagation.
    /// This overload allows binding directly from Result.Success() to an async operation that returns a value.
    /// </summary>
    public static async Task<Result<T>> BindAsync<T>(
        this Result result,
        Func<Task<Result<T>>> bindFunc) where T : notnull
    {
        return result.IsSuccess
            ? await bindFunc().ConfigureAwait(false)
            : Result.Failure<T>(result.Error);
    }

    /// <summary>
    /// Asynchronously chains operations that return Result types, allowing for error propagation.
    /// </summary>
    public static async Task<Result<T>> BindAsync<T>(
        this Task<Result> resultTask,
        Func<Task<Result<T>>> bindFunc) where T : notnull
    {
        Result result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess
            ? await bindFunc().ConfigureAwait(false)
            : Result.Failure<T>(result.Error);
    }

    /// <summary>
    /// Chains operations that return Result types, allowing for error propagation.
    /// </summary>
    public static Result Bind<TSource>(
        this Result<TSource> result,
        Func<TSource, Result> bindFunc) where TSource : notnull
    {
        return result.IsSuccess
            ? bindFunc(result.Value)
            : Result.Failure(result.Error);
    }

    /// <summary>
    /// Asynchronously chains operations that return Result types, allowing for error propagation.
    /// </summary>
    public static async Task<Result> BindAsync<TSource>(
        this Task<Result<TSource>> resultTask,
        Func<TSource, Task<Result>> bindFunc) where TSource : notnull
    {
        Result<TSource> result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess
            ? await bindFunc(result.Value).ConfigureAwait(false)
            : Result.Failure(result.Error);
    }

    /// <summary>
    /// Executes an action when the result is successful without modifying it and returns the original result.
    /// </summary>
    public static Result<T> OnSuccess<T>(
        this Result<T> result,
        Action<T> successAction) where T : notnull
    {
        if (result.IsSuccess)
        {
            successAction(result.Value);
        }

        return result;
    }

    /// <summary>
    /// Asynchronously executes an action when the result is successful without modifying it and returns the original result.
    /// </summary>
    public static async Task<Result<T>> OnSuccessAsync<T>(
        this Task<Result<T>> resultTask,
        Func<T, Task> successAction) where T : notnull
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess)
        {
            await successAction(result.Value);
        }

        return result;
    }

    /// <summary>
    /// Executes an action when the result is successful without modifying it and returns the original result.
    /// </summary>
    public static Result OnSuccess(
        this Result result,
        Action successAction)
    {
        if (result.IsSuccess)
        {
            successAction();
        }

        return result;
    }

    /// <summary>
    /// Asynchronously executes an action when the result is successful without modifying it and returns the original result.
    /// </summary>
    public static async Task<Result> OnSuccessAsync(
        this Task<Result> resultTask,
        Func<Task> successAction)
    {
        Result result = await resultTask.ConfigureAwait(false);
        if (result.IsSuccess)
        {
            await successAction().ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Executes an action when the result is a failure without modifying it.
    /// </summary>
    public static Result<T> OnFailure<T>(
        this Result<T> result,
        Action<Error> failureAction) where T : notnull
    {
        if (!result.IsSuccess)
        {
            failureAction(result.Error);
        }
        return result;
    }

    /// <summary>
    /// Asynchronously executes an action when the result is a failure without modifying it.
    /// </summary>
    public static async Task<Result<T>> OnFailureAsync<T>(
        this Task<Result<T>> resultTask,
        Func<Error, Task> failureAction) where T : notnull
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await failureAction(result.Error).ConfigureAwait(false);
        }

        return result;
    }

    /// <summary>
    /// Executes an action when the result is a failure without modifying it.
    /// </summary>
    public static Result OnFailure(
        this Result result,
        Action<Error> failureAction)
    {
        if (!result.IsSuccess)
        {
            failureAction(result.Error);
        }
        return result;
    }

    /// <summary>
    /// Asynchronously executes an action when the result is a failure without modifying it.
    /// </summary>
    public static async Task<Result> OnFailureAsync(
        this Task<Result> resultTask,
        Func<Error, Task> failureAction)
    {
        Result result = await resultTask.ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            await failureAction(result.Error).ConfigureAwait(false);
        }
        return result;
    }

    /// <summary>
    /// Wraps a value in a successful Result.
    /// </summary>
    public static Result<T> ToResult<T>(this T value) where T : notnull
    {
        return Result.Success(value);
    }

    /// <summary>
    /// Retrieves the value from a successful result, throwing an exception if it is a failure.
    /// </summary>
    public static T Unwrap<T>(this Result<T> result) where T : notnull
    {
        return result.IsSuccess
            ? result.Value
            : throw new InvalidOperationException(CreateUnwrapFailureMessage(result.Error));
    }

    /// <summary>
    /// Ensures the result is successful, throwing an exception if it is a failure.
    /// </summary>
    /// <param name="result"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void Unwrap(this Result result)
    {
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(CreateUnwrapFailureMessage(result.Error));
        }
    }

    /// <summary>
    /// Asynchronously retrieves the value from a successful result, throwing an exception if it is a failure.
    /// </summary>
    public static async Task<T> UnwrapAsync<T>(this Task<Result<T>> resultTask) where T : notnull
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess
            ? result.Value
            : throw new InvalidOperationException(CreateUnwrapFailureMessage(result.Error));
    }

    /// <summary>
    /// Asynchronously ensures the result is successful, throwing an exception if it is a failure.
    /// </summary>
    /// <param name="resultTask"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task UnwrapAsync(this Task<Result> resultTask)
    {
        Result result = await resultTask.ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(CreateUnwrapFailureMessage(result.Error));
        }
    }

    /// <summary>
    /// Retrieves the value from a successful result, or returns the provided default value if it is a failure.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static T UnwrapOr<T>(this Result<T> result, T defaultValue) where T : notnull
    {
        return result.IsSuccess ? result.Value : defaultValue;
    }

    /// <summary>
    /// Asynchronously retrieves the value from a successful result, or returns the provided default value if it is a failure.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resultTask"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static async Task<T> UnwrapOrAsync<T>(this Task<Result<T>> resultTask, T defaultValue) where T : notnull
    {
        Result<T> result = await resultTask.ConfigureAwait(false);
        return result.IsSuccess ? result.Value : defaultValue;
    }

    /// <summary>
    /// Executes the appropriate function based on success or failure.
    /// </summary>
    public static TResult Match<T, TResult>(
        this Result<T> result,
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure) where T : notnull
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);
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

    private static string CreateUnwrapFailureMessage(Error error)
    {
        return $"Cannot unwrap a failure result. Error Code: {error.Code}, Message: {error.Message}";
    }
}
