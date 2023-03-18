using Klab.Toolkit.Common.Entities;

namespace Klab.Toolkit.Common;

/// <summary>
/// Interface for a retry
/// </summary>
public interface IRetryService
{
    /// <summary>
    /// Try to call the given callback.
    ///
    /// If the callback throws an exception, it will be retried. If the callback
    /// succeeds, the result will be returned.
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="retryCount"></param>
    /// <returns>result if the call was successful</returns>
    Result TryCall(Action callback, int retryCount = 3);
}

/// <summary>
/// Implementation of <see cref="IRetryService"/>.
/// </summary>
public class RetryService : IRetryService
{
    /// <inheritdoc/>
    public Result TryCall(Action callback, int retryCount = 3)
    {
        int numberOfRetries = retryCount;
        Result result = Result.Failure(Error.None);

        while (numberOfRetries > 0)
        {
            try
            {
                callback();
                return Result.Success();
            }
            catch (Exception e)
            {
                result = Result.Failure(e);
            }

            numberOfRetries--;
        }

        return result;
    }
}
