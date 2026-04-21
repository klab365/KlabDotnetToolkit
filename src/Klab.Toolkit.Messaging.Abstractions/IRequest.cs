namespace Klab.Toolkit.Messaging;

/// <summary>
/// Marker interface for requests with response
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public interface IRequest<out TResponse>
{
}
