// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This interface maintains the structure for working with RabbitMQ
/// </summary>
/// /// <typeparam name="TPayload">The expected message type</typeparam>
public interface IQueueService<TPayload> : IQueueService
{
    /// <summary>
    /// This method asynchronously publishes a message a queue
    /// </summary>
    /// <param name="payload">The message payload to publish</param>
    /// <returns>An awaitable task containing the published message</returns>
    public Task<QueueMessage<TPayload>> PublishAsync(TPayload payload);

    /// <summary>
    /// This method asynchronously publishes a message to <paramref name="queueName" />
    /// </summary>
    /// <param name="queueName">The queue to publish the <paramref name="payload" /> to</param>
    /// <param name="payload">The message payload to publish</param>
    /// <returns>An awaitable task containing the published message</returns>
    public Task<QueueMessage<TPayload>> PublishAsync(string queueName, TPayload payload);

    /// <summary>
    /// This method asynchronously subscribes to the queue
    /// </summary>
    /// <param name="delegateSubscriber">The message worker</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <returns>An awaitable task containing the message</returns>
    public Task SubscribeAsync(DelegateSubscriberAsync<TPayload> delegateSubscriber,
        CancellationToken stoppingToken = default);

    /// <summary>
    /// This method asynchronously subscribes to <paramref name="queueName" />
    /// </summary>
    /// <param name="queueName">The queue to subscribe to</param>
    /// <param name="delegateSubscriber">The message worker</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    /// <returns>An awaitable task containing the message</returns>
    public Task SubscribeAsync(string queueName, DelegateSubscriberAsync<TPayload> delegateSubscriber,
        CancellationToken stoppingToken = default);
}
