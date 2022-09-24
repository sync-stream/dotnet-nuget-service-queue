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
    /// This method instantiates our service with a RabbitMQ Connection
    /// </summary>
    /// <param name="logServiceProvider">The log service provider</param>
    /// <param name="defaultEndpoint">Optional default queue endpoint to use</param>
    /// <param name="defaultSimpleStorageServiceConfiguration">The default S3 configuration for the queue</param>
    public QueueService(ILogger<QueueService<TPayload>> logServiceProvider, QueueConfiguration defaultEndpoint = null,
        QueueSimpleStorageServiceConfiguration defaultSimpleStorageServiceConfiguration = null) : base(
        logServiceProvider,
        defaultEndpoint, defaultSimpleStorageServiceConfiguration)
    {
    }

    /// <summary>
    /// This method instantiates our service with a RabbitMQ Connection
    /// </summary>
    /// <param name="logServiceProvider">The log service provider</param>
    /// <param name="defaultEndpoint">Optional default queue endpoint to use</param>
    /// <param name="defaultSimpleStorageServiceConfiguration">The default S3 configuration for the queue</param>
    public QueueService(ILogger<QueueService<TPayload>> logServiceProvider, string defaultEndpoint = null,
        QueueSimpleStorageServiceConfiguration defaultSimpleStorageServiceConfiguration = null) : base(
        logServiceProvider,
        defaultEndpoint, defaultSimpleStorageServiceConfiguration)
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
}
