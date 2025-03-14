using System;
using System.Threading;
using System.Threading.Tasks;
using Klab.Toolkit.Results;

namespace Klab.Toolkit.Common;

/// <summary>
/// Implementation of <see cref="ITimeProvider"/> that uses the system clock.
/// </summary>
public class TimeProvider : ITimeProvider
{
    /// <inheritdoc />
    public DateTimeOffset GetCurrentTime()
    {
        return new DateTimeOffset(DateTime.UtcNow);
    }

    /// <summary>
    /// Wait for the specified time span.
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Result> WaitAsync(TimeSpan timeSpan, CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(timeSpan, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            return Result.Failure(Error.Create(string.Empty, "Wait operation was canceled."));
        }

        return Result.Success();
    }
}
