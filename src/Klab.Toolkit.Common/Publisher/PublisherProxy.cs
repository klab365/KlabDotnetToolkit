namespace Klab.Toolkit.Common.Publisher;

/// <summary>
/// Interface for a publisher proxy
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IPublisherProxy<T>
{
    /// <summary>
    /// Event for new element published
    /// </summary>
    event EventHandler<T>? NewElementPublished;

    /// <summary>
    /// Publish message
    /// </summary>
    /// <param name="message"></param>
    void Publish(T? message);
}

/// <summary>
/// Implementation of the interface <see cref="IPublisherProxy{T}"/>
/// </summary>
/// <typeparam name="T"></typeparam>
public class PublisherProxy<T> : IPublisherProxy<T>
{
    /// <inheritdoc/>
    public event EventHandler<T>? NewElementPublished;

    /// <inheritdoc/>
    public void Publish(T? message)
    {
        if (message is null)
        {
            return;
        }

        NewElementPublished?.Invoke(this, message);
    }
}
