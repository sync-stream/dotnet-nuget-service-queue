// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This interface maintains the structure for working with RabbitMQ
/// </summary>
/// /// <typeparam name="TPayload">The expected message type</typeparam>
public interface IQueueService<TPayload> : IQueueService
{
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
