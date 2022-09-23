// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This interface maintains the structure for working with RabbitMQ
/// </summary>
/// /// <typeparam name="TPayload">The expected message type</typeparam>
public interface IQueueService<TPayload> : IQueueService
{
    /// <summary>
    /// This delegate provides the structure fo a subscriber
    /// </summary>
    /// <param name="message">The message itself</param>
    public delegate void DelegateSubscriber(QueueMessage<TPayload> message);

    /// <summary>
    /// This delegate provides the structure for an asynchronous subscriber
    /// </summary>
    /// <param name="message">The message itself</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    public delegate Task DelegateSubscriberAsync(QueueMessage<TPayload> message,
        CancellationToken stoppingToken = default);

    /// <summary>
    /// This method publishes a <paramref name="payload"/> to the queue
    /// </summary>
    /// <param name="payload">The message to publish</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    /// <returns>The message that was published</returns>
    public QueueMessage<TPayload> Publish(TPayload payload);

    /// <summary>
    /// This method publishes a message to <paramref name="queueName" />
    /// </summary>
    /// <param name="queueName">The queue to publish messages to</param>
    /// <param name="payload">The message to publish</param>
    /// <returns>The published message</returns>
    public QueueMessage<TPayload> Publish(string queueName, TPayload payload);

    /// <summary>
    /// This method subscribes to the queue
    /// </summary>
    /// <param name="delegateSubscriber">The subscription worker</param>
    public void Subscribe(DelegateSubscriber<TPayload> delegateSubscriber);

    /// <summary>
    /// This method subscribes to <paramref name="queueName" />
    /// </summary>
    /// <param name="queueName">The queue to subscribe to</param>
    /// <param name="delegateSubscriber">The subscription worker</param>
    public void Subscribe(string queueName, DelegateSubscriber<TPayload> delegateSubscriber);

    /// <summary>
    /// This method asynchronously subscribes to the queue
    /// </summary>
    /// <param name="delegateSubscriber">The message worker</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <returns>An awaitable task containing the message</returns>
    public void Subscribe(DelegateSubscriberAsync<TPayload> delegateSubscriber,
        CancellationToken stoppingToken = default);

    /// <summary>
    /// This method asynchronously subscribes to <paramref name="queueName" />
    /// </summary>
    /// <param name="queueName">The queue to subscribe to</param>
    /// <param name="delegateSubscriber">The message worker</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    /// <returns>An awaitable task containing the message</returns>
    public void Subscribe(string queueName, DelegateSubscriberAsync<TPayload> delegateSubscriber,
        CancellationToken stoppingToken = default);
}
