// Define our namespace
namespace SyncStream.Service.Queue.RabbitMq;

/// <summary>
/// This interface maintains the structure for working with RabbitMQ
/// </summary>
public interface IRabbitMqService
{
    /// <summary>
    /// This delegate provides the structure fo a subscriber
    /// </summary>
    /// <param name="message">The message itself</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    public delegate void DelegateSubscriber<TPayload>(RabbitMqMessage<TPayload> message);

    /// <summary>
    /// This delegate provides the structure for an asynchronous subscriber
    /// </summary>
    /// <param name="message">The message itself</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    public delegate Task DelegateSubscriberAsync<TPayload>(RabbitMqMessage<TPayload> message,
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
    /// This method publishes a <paramref name="payload"/> to the queue
    /// </summary>
    /// <param name="payload">The message to publish</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    /// <returns>The message that was published</returns>
    public RabbitMqMessage<TPayload> Publish<TPayload>(TPayload payload);

    /// <summary>
    /// This method publishes a message to <paramref name="queueName" />
    /// </summary>
    /// <param name="queueName">The queue to publish messages to</param>
    /// <param name="payload">The message to publish</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    /// <returns>The published message</returns>
    public RabbitMqMessage<TPayload> Publish<TPayload>(string queueName, TPayload payload);

    /// <summary>
    /// This method registers a RabbitMQ endpoint
    /// </summary>
    /// <param name="endpoint">The RabbitMQ host and queue details</param>
    /// <returns>This instance</returns>
    public IRabbitMqService RegisterEndpoint(RabbitMqQueueConfiguration endpoint);

    /// <summary>
    /// This method registers a range of RabbitMQ endpoints
    /// </summary>
    /// <param name="endpoints">The RabbitMQ host and queue details</param>
    /// <returns>This instance</returns>
    public IRabbitMqService RegisterEndpoints(IEnumerable<RabbitMqQueueConfiguration> endpoints);

    /// <summary>
    /// This method registers a range of RabbitMQ endpoints
    /// </summary>
    /// <param name="endpoints">The RabbitMQ host and queue details</param>
    /// <returns>This instance</returns>
    public IRabbitMqService RegisterEndpoints(params RabbitMqQueueConfiguration[] endpoints);

    /// <summary>
    /// This method subscribes to the queue
    /// </summary>
    /// <param name="delegateSubscriber">The subscription worker</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    public void Subscribe<TPayload>(DelegateSubscriber<TPayload> delegateSubscriber);

    /// <summary>
    /// This method subscribes to <paramref name="queueName" />
    /// </summary>
    /// <param name="queueName">The queue to subscribe to</param>
    /// <param name="delegateSubscriber">The subscription worker</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    public void Subscribe<TPayload>(string queueName, DelegateSubscriber<TPayload> delegateSubscriber);

    /// <summary>
    /// This method asynchronously subscribes to the queue
    /// </summary>
    /// <param name="delegateSubscriber">The message worker</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    /// <returns>An awaitable task containing the message</returns>
    public void Subscribe<TPayload>(DelegateSubscriberAsync<TPayload> delegateSubscriber,
        CancellationToken stoppingToken);

    /// <summary>
    /// This method asynchronously subscribes to <paramref name="queueName" />
    /// </summary>
    /// <param name="queueName">The queue to subscribe to</param>
    /// <param name="delegateSubscriber">The message worker</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    /// <returns>An awaitable task containing the message</returns>
    public void Subscribe<TPayload>(string queueName,
        DelegateSubscriberAsync<TPayload> delegateSubscriber, CancellationToken stoppingToken = default);

    /// <summary>
    /// This method fluidly resets the queue's name into the instance
    /// </summary>
    /// <param name="queueName">The queue to publish and subscribe to</param>
    /// <returns>This instance</returns>
    public IRabbitMqService WithDefaultEndpoint(string queueName);

    /// <summary>
    /// This method fluidly resets the default queue into the service
    /// </summary>
    /// <param name="endpoint">The queue configuration to use by default</param>
    /// <param name="register">Denotes whether to register the endpoint if it doesn't exist or not</param>
    /// <returns>This instance</returns>
    public IRabbitMqService WithDefaultEndpoint(RabbitMqQueueConfiguration endpoint, bool register = true);
}
