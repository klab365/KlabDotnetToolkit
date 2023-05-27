namespace Klab.Toolkit.Common.Publisher;

/// <summary>
/// Interface for a publisher
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IPublisher<in T>
{
    /// <summary>
    /// Publish message
    /// </summary>
    /// <param name="message"></param>
    void Publish(T message);
}

/// <summary>
/// Implementation of the interface <see cref="IPublisher{T}"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public class Publisher<T> : IPublisher<T>
{
    private readonly IPublisherProxy<T> _publisherProxy;

    /// <summary>
    /// Create a instance of the class <see cref="Publisher{T}"/>
    /// </summary>
    /// <param name="publisherProxy"></param>
    public Publisher(IPublisherProxy<T> publisherProxy)
    {
        _publisherProxy = publisherProxy;
    }

    /// <summary>
    /// Publish message
    /// </summary>
    /// <param name="message"></param>
    public virtual void Publish(T message)
    {
        _publisherProxy.Publish(message);
    }
}
