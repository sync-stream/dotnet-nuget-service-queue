// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This interface maintains the structure for working with RabbitMQ
/// </summary>
public interface IQueueService
{
    /// <summary>
    /// This delegate provides the structure for an asynchronous subscriber
    /// </summary>
    /// <param name="message">The message itself</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    public delegate Task DelegateSubscriberAsync<TPayload>(QueueMessage<TPayload> message,
        CancellationToken stoppingToken = default);

    /// <summary>
    /// This method disconnects from the queue
    /// </summary>
    /// <param name="all">Denotes whether to disconnect all queues or not</param>
    public void Disconnect(bool all = false);

    /// <summary>
    /// This method returns the total messages on <paramref name="queueName" />
    /// </summary>
    /// <param name="queueName">The queue to query</param>
    /// <returns></returns>
    public uint MessageCount(string queueName = null);

    /// <summary>
    /// This method asynchronously publishes a message to the queue and optionally to S3
    /// </summary>
    /// <param name="payload">The content of the message to publish</param>
    /// <typeparam name="TPayload">The expected type of the message payload</typeparam>
    /// <returns>An awaitable task containing the published message</returns>
    public Task<QueueMessage<TPayload>> PublishAsync<TPayload>(TPayload payload);

    /// <summary>
    /// This method asynchronously publishes a message to <paramref name="queueName"/> and optionally to S3
    /// </summary>
    /// <param name="queueName">The queue to publish the message to</param>
    /// <param name="payload">The content of the message to publish</param>
    /// <typeparam name="TPayload">The expected type of the message payload</typeparam>
    /// <returns>An awaitable task containing the published message</returns>
    public Task<QueueMessage<TPayload>> PublishAsync<TPayload>(string queueName, TPayload payload);

    /// <summary>
    /// This method registers a RabbitMQ endpoint
    /// </summary>
    /// <param name="endpoint">The RabbitMQ host and queue details</param>
    /// <returns>This instance</returns>
    public IQueueService RegisterEndpoint(QueueConfiguration endpoint);

    /// <summary>
    /// This method registers a range of RabbitMQ endpoints
    /// </summary>
    /// <param name="endpoints">The RabbitMQ host and queue details</param>
    /// <returns>This instance</returns>
    public IQueueService RegisterEndpoints(IEnumerable<QueueConfiguration> endpoints);

    /// <summary>
    /// This method registers a range of RabbitMQ endpoints
    /// </summary>
    /// <param name="endpoints">The RabbitMQ host and queue details</param>
    /// <returns>This instance</returns>
    public IQueueService RegisterEndpoints(params QueueConfiguration[] endpoints);

    /// <summary>
    /// This method asynchronously subscribes to the queue
    /// </summary>
    /// <param name="delegateSubscriber">The message worker</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    /// <returns>An awaitable task containing the message</returns>
    public Task SubscribeAsync<TPayload>(DelegateSubscriberAsync<TPayload> delegateSubscriber,
        CancellationToken stoppingToken);

    /// <summary>
    /// This method asynchronously subscribes to <paramref name="queueName" />
    /// </summary>
    /// <param name="queueName">The queue to subscribe to</param>
    /// <param name="delegateSubscriber">The message worker</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    /// <returns>An awaitable task containing the message</returns>
    public Task SubscribeAsync<TPayload>(string queueName,
        DelegateSubscriberAsync<TPayload> delegateSubscriber, CancellationToken stoppingToken = default);

    /// <summary>
    /// This method fluidly resets the queue's name into the instance
    /// </summary>
    /// <param name="queueName">The queue to publish and subscribe to</param>
    /// <returns>This instance</returns>
    public IQueueService WithDefaultEndpoint(string queueName);

    /// <summary>
    /// This method fluidly resets the default queue into the service
    /// </summary>
    /// <param name="endpoint">The queue configuration to use by default</param>
    /// <param name="register">Denotes whether to register the endpoint if it doesn't exist or not</param>
    /// <returns>This instance</returns>
    public IQueueService WithDefaultEndpoint(QueueConfiguration endpoint, bool register = true);

    /// <summary>
    /// This method provides a <paramref name="simpleStorageServiceConfiguration" /> for using AWS S3 in
    /// conjunction with RabbitMQ to reduce message size and durability
    /// </summary>
    /// <param name="simpleStorageServiceConfiguration">The AWS S3 configuration details</param>
    /// <returns></returns>
    public IQueueService WithSimpleStorageService(
        QueueSimpleStorageServiceConfiguration simpleStorageServiceConfiguration);
}
