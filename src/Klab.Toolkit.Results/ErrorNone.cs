using System.Threading.Tasks;

namespace Klab.Toolkit.Results;

/// <summary>
/// None Error Type
/// </summary>
public record ErrorNone : IError
{
    /// <inheritdoc/>
    public string Code => string.Empty;

    /// <inheritdoc/>
    public string Message => string.Empty;

    /// <inheritdoc/>
    public string Advice => string.Empty;

    /// <inheritdoc/>
    public string StackTrace => string.Empty;

    /// <inheritdoc/>
    public bool ShouldQueue => false;

    /// <inheritdoc/>
    public Task<bool> IsPendingAsyc()
    {
        return Task.FromResult(false);
    }
}
