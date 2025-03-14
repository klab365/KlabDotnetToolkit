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
    /// <inheritdoc/>
    public async Task<Result> TryCallAsync(Func<CancellationToken, Task> callback, TimeSpan timeout, int retryCount = 3)
    {
        int numberOfRetries = retryCount;
        Result? result = null;

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
                Error error = Error.FromException("RetryService", ErrorType.Warning, e);
                result = Result.Failure(error);
            }

            numberOfRetries--;
        }

        return result!;
    }
}
