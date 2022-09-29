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
    /// This method fluidly resets the queue configuration into the instance
    /// </summary>
    /// <param name="configuration">The queue encryption configuration</param>
    /// <returns>The current instance</returns>
    public IQueueService SetQueueEncryptionConfiguration(QueueServiceEncryptionConfiguration configuration);

    /// <summary>
    /// This method fluidly resets the existing queue in the instance
    /// </summary>
    /// <param name="endpoint">The new queue endpoint to use by default</param>
    /// <returns>The current instance</returns>
    public IQueueService SetQueueEndpoint(QueueConfiguration endpoint);

    /// <summary>
    /// This method fluidly resets the existing S3 configuration into the instance
    /// </summary>
    /// <param name="simpleStorageServiceConfiguration">The new S3 configuration to use by default</param>
    /// <returns>The current instance</returns>
    public IQueueService SetQueueSimpleStorageServiceConfiguration(
        QueueSimpleStorageServiceConfiguration simpleStorageServiceConfiguration);

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
    ///     This method fluidly resets the queue's encryption configuration into the instance
    /// </summary>
    /// <param name="encryptionConfiguration">The encryption configuration to use</param>
    /// <returns>The current instance</returns>
    public IQueueService UseEncryption(QueueServiceEncryptionConfiguration encryptionConfiguration);

    /// <summary>
    ///     This method fluidly resets the queue endpoint into the instance
    /// </summary>
    /// <param name="queueEndpointConfiguration">The queue endpoint configuration to use</param>
    /// <returns>The current instance</returns>
    public IQueueService UseEndpoint(QueueConfiguration queueEndpointConfiguration);

    /// <summary>
    ///     This method fluidly resets the queue endpoint into the instance
    /// </summary>
    /// <param name="queueEndpoint">The name of the queue endpoint configuration to use</param>
    /// <returns>The current instance</returns>
    public IQueueService UseEndpoint(string queueEndpoint);

    /// <summary>
    ///     This method fluidly resets the queue's S3 configuration into the instance
    /// </summary>
    /// <param name="simpleStorageServiceConfiguration">The S3 configuration to use</param>
    /// <returns>The current instance</returns>
    public IQueueService UseSimpleStorageService(
        QueueSimpleStorageServiceConfiguration simpleStorageServiceConfiguration);
}
