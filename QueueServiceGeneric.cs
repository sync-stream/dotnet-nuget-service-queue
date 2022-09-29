using Microsoft.Extensions.Logging;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This service is responsible for queueing things
/// </summary>
public class QueueService<TPayload> : QueueService, IQueueService<TPayload>
{
    /// <summary>
    /// This method instantiates our service with a RabbitMQ Connection
    /// </summary>
    /// <param name="logServiceProvider">The log service provider</param>
    public QueueService(ILogger<QueueService<TPayload>> logServiceProvider) : base(logServiceProvider)
    {
    }

    /// <summary>
    /// This method asynchronously publishes a message a queue
    /// </summary>
    /// <param name="payload">The message payload to publish</param>
    /// <returns>An awaitable task containing the published message</returns>
    public Task<QueueMessage<TPayload>> PublishAsync(TPayload payload) => base.PublishAsync(payload);

    /// <summary>
    /// This method asynchronously publishes a message to <paramref name="queueName" />
    /// </summary>
    /// <param name="queueName">The queue to publish the <paramref name="payload" /> to</param>
    /// <param name="payload">The message payload to publish</param>
    /// <returns>An awaitable task containing the published message</returns>
    public Task<QueueMessage<TPayload>> PublishAsync(string queueName, TPayload payload) =>
        base.PublishAsync(queueName, payload);

    /// <summary>
    /// This method asynchronously subscribes to the queue
    /// </summary>
    /// <param name="delegateSubscriber">The message worker</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <returns>An awaitable task containing the message</returns>
    public Task SubscribeAsync(IQueueService.DelegateSubscriberAsync<TPayload> delegateSubscriber,
        CancellationToken stoppingToken = default) => base.SubscribeAsync(delegateSubscriber, stoppingToken);

    /// <summary>
    /// This method asynchronously subscribes to <paramref name="queueName" />
    /// </summary>
    /// <param name="queueName">The queue to subscribe to</param>
    /// <param name="delegateSubscriber">The message worker</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    /// <returns>An awaitable task containing the message</returns>
    public Task SubscribeAsync(string queueName, IQueueService.DelegateSubscriberAsync<TPayload> delegateSubscriber,
        CancellationToken stoppingToken = default) => base.SubscribeAsync(queueName, delegateSubscriber, stoppingToken);

    /// <summary>
    ///     This method fluidly resets the queue's encryption configuration into the instance
    /// </summary>
    /// <param name="encryptionConfiguration">The encryption configuration to use</param>
    /// <returns>The current instance</returns>
    public new IQueueService<TPayload> UseEncryption(QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        base.UseEncryption(encryptionConfiguration) as IQueueService<TPayload>;

    /// <summary>
    ///     This method fluidly resets the queue endpoint into the instance
    /// </summary>
    /// <param name="queueEndpointConfiguration">The queue endpoint configuration to use</param>
    /// <returns>The current instance</returns>
    public new IQueueService<TPayload> UseEndpoint(QueueConfiguration queueEndpointConfiguration) =>
        base.UseEndpoint(queueEndpointConfiguration) as IQueueService<TPayload>;

    /// <summary>
    ///     This method fluidly resets the queue's S3 configuration into the instance
    /// </summary>
    /// <param name="simpleStorageServiceConfiguration">The S3 configuration to use</param>
    /// <returns>The current instance</returns>
    public new IQueueService<TPayload> UseSimpleStorageService(
        QueueSimpleStorageServiceConfiguration simpleStorageServiceConfiguration) =>
        base.UseSimpleStorageService(simpleStorageServiceConfiguration) as IQueueService<TPayload>;

    /// <summary>
    ///     This method fluidly resets the queue endpoint into the instance
    /// </summary>
    /// <param name="queueEndpoint">The name of the queue endpoint configuration to use</param>
    /// <returns>The current instance</returns>
    public new IQueueService<TPayload> UseEndpoint(string queueEndpoint) =>
        base.UseEndpoint(queueEndpoint) as IQueueService<TPayload>;
}
