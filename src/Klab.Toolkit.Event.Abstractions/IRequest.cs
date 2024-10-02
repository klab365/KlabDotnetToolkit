namespace Klab.Toolkit.Event;

/// <summary>
/// Marker interface for requests
/// </summary>
public interface IRequest
{
}

/// <summary>
/// Marker interface for requests with response
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public interface IRequest<out TResponse>
{
}
