using System;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;

namespace Klab.Toolkit.Common;

/// <summary>
/// Implementation of <see cref="IRetryService"/>.
/// </summary>
public class RetryService : IRetryService
{
    private const int ErrorId = 9999;

    /// <inheritdoc/>
    public async Task<Result> TryCallAsync(Func<CancellationToken, Task> callback, TimeSpan timeout, int retryCount = 3)
    {
        int numberOfRetries = retryCount;
        Result result = Result.Failure(Error.None);

        while (numberOfRetries > 0)
        {
            try
            {
                using CancellationTokenSource cancellationTokenSource = new(timeout);
                await callback(cancellationTokenSource.Token);
                return Result.Success();
            }
            catch (Exception e)
            {
                Error error = Error.FromException(ErrorId, e);
                result = error;
            }

            numberOfRetries--;
        }

        return result;
    }
}
