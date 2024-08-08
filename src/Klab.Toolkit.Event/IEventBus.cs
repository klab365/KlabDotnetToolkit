using System.Threading;
using System.Threading.Tasks;

namespace Klab.Toolkit.Event;

/// <summary>
/// Represents a message bus
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an event and not wait until event is processed.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="event"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;

    //Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
    //    where TRequest : ICommand<TResponse>;
}

//public interface ICommand
//{
//}
