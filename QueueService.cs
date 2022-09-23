using Microsoft.Extensions.Logging;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This service is responsible for queueing things
/// </summary>
public class QueueService : IQueueService
{
    /// <summary>
    /// This property contains the list of available queues
    /// </summary>
    private static readonly List<QueueConfiguration> Queues = new();

    /// <summary>
    /// This method returns a queue endpoint configuration by it's name or endpoint
    /// </summary>
    /// <param name="queueName">The queue name or endpoint</param>
    /// <returns>The queue with the name or endpoint <paramref name="queueName" /> or null</returns>
    public static QueueConfiguration GetEndpointConfiguration(string queueName) =>
        Queues.FirstOrDefault(q =>
            q.Endpoint.ToLower().Equals(queueName.ToLower()) || q.Name.ToLower().Equals(queueName.ToLower()));

    /// <summary>
    /// This method registers a RabbitMQ endpoint configuration
    /// </summary>
    /// <param name="endpoint">The RabbitMQ host and queue details</param>
    /// <returns>This instance</returns>
    public static void RegisterEndpointConfiguration(QueueConfiguration endpoint)
    {
        // Ensure we're not trying to duplicate endpoints
        if (GetEndpointConfiguration(endpoint.Endpoint) is not null) return;

        // Add the endpoint to the instance
        Queues.Add(endpoint);
    }

    /// <summary>
    /// This method registers a range of RabbitMQ endpoints
    /// </summary>
    /// <param name="endpoints">The RabbitMQ host and queue details</param>
    /// <returns>This instance</returns>
    public static void RegisterEndpointConfigurations(IEnumerable<QueueConfiguration> endpoints) =>
        endpoints.ToList().ForEach(RegisterEndpointConfiguration);

    /// <summary>
    /// This method registers a range of RabbitMQ endpoints
    /// </summary>
    /// <param name="endpoints">The RabbitMQ host and queue details</param>
    /// <returns>This instance</returns>
    public static void RegisterEndpointConfigurations(params QueueConfiguration[] endpoints) =>
        RegisterEndpointConfigurations(endpoints.ToList());

    /// <summary>
    /// This property contains the instance of our logger
    /// </summary>
    private readonly ILogger<QueueService> _logger;

    /// <summary>
    /// This property contains the instance of our default queue
    /// </summary>
    private QueueConfiguration _queue;

    /// <summary>
    /// This property contains the instance of our simple storage service configuration
    /// </summary>
    private QueueSimpleStorageServiceConfiguration _simpleStorageService;

    /// <summary>
    /// This method instantiates our service with a RabbitMQ Connection
    /// </summary>
    /// <param name="logServiceProvider">The log service provider</param>
    /// <param name="defaultEndpoint">Optional default queue endpoint to use</param>
    /// <param name="defaultSimpleStorageService">The default S3 configuration for the queue</param>
    public QueueService(ILogger<QueueService> logServiceProvider, QueueConfiguration defaultEndpoint = null,
        QueueSimpleStorageServiceConfiguration defaultSimpleStorageService = null)
    {
        // Set the logger into the instance
        _logger = logServiceProvider;

        // Check for a default queue
        if (defaultEndpoint is not null) WithDefaultEndpoint(defaultEndpoint);

        // Check for an S3 configuration
        if (defaultSimpleStorageService is not null)
            _simpleStorageService = defaultSimpleStorageService;
    }

    /// <summary>
    /// This method instantiates our service with a RabbitMQ Connection
    /// </summary>
    /// <param name="logServiceProvider">The log service provider</param>
    /// <param name="defaultEndpoint">Optional default queue endpoint to use</param>
    /// <param name="defaultSimpleStorageService">The default S3 configuration for the queue</param>
    public QueueService(ILogger<QueueService> logServiceProvider, string defaultEndpoint = null,
        QueueSimpleStorageServiceConfiguration defaultSimpleStorageService = null)
    {
        // Set the logger into the instance
        _logger = logServiceProvider;

        // Check for a default queue
        if (defaultEndpoint is not null) WithDefaultEndpoint(defaultEndpoint);

        // Check for an S3 configuration
        if (defaultSimpleStorageService is not null)
            _simpleStorageService = defaultSimpleStorageService;
    }

    /// <summary>
    /// This method disconnects from the queue
    /// </summary>
    /// <param name="all">Denotes whether to disconnect all queues or not</param>
    public void Disconnect(bool all = false)
    {
        // Disconnect from the queue endpoint
        _queue?.Disconnect();

        // Iterate over the queues in the instance
        if (all) Queues.ForEach(q => q?.Disconnect());
    }

    /// <summary>
    /// This method returns the total messages on the queue
    /// </summary>
    /// <param name="queueName"></param>
    /// <returns></returns>
    public uint MessageCount(string queueName = null) =>
        (queueName is null
            ? _queue?.GetChannel().MessageCount(_queue?.Endpoint)
            : GetEndpointConfiguration(queueName)?.GetChannel()
                ?.MessageCount(GetEndpointConfiguration(queueName)?.Endpoint)) ?? 0;

    /// <summary>
    /// This method asynchronously publishes a message to the queue and optionally to S3
    /// </summary>
    /// <param name="payload">The content of the message to publish</param>
    /// <typeparam name="TPayload">The expected type of the message payload</typeparam>
    /// <returns>An awaitable task containing the published message</returns>
    public Task<QueueMessage<TPayload>> PublishAsync<TPayload>(TPayload payload)
    {
        // Instantiate our publisher
        QueuePublisher<TPayload> publisher = new(_logger as ILogger<QueuePublisher<TPayload>>, _queue.GetChannel(),
            _queue.Endpoint, _queue.SimpleStorageService ?? _simpleStorageService);

        // We're done, publish the message
        return publisher.PublishAsync(payload);
    }

    /// <summary>
    /// This method asynchronously publishes a message to <paramref name="queueName"/> and optionally to S3
    /// </summary>
    /// <param name="queueName">The queue to publish the message to</param>
    /// <param name="payload">The content of the message to publish</param>
    /// <typeparam name="TPayload">The expected type of the message payload</typeparam>
    /// <returns>An awaitable task containing the published message</returns>
    public Task<QueueMessage<TPayload>> PublishAsync<TPayload>(string queueName, TPayload payload) =>
        WithDefaultEndpoint(queueName).PublishAsync(payload);

    /// <summary>
    /// This method registers a RabbitMQ endpoint
    /// </summary>
    /// <param name="endpoint">The RabbitMQ host and queue details</param>
    /// <returns>This instance</returns>
    public IQueueService RegisterEndpoint(QueueConfiguration endpoint)
    {
        // Register the endpoint if it doesn't exist
        RegisterEndpointConfiguration(endpoint);

        // We're done, return the instance
        return this;
    }

    /// <summary>
    /// This method registers a range of RabbitMQ endpoints
    /// </summary>
    /// <param name="endpoints">The RabbitMQ host and queue details</param>
    /// <returns>This instance</returns>
    public IQueueService RegisterEndpoints(IEnumerable<QueueConfiguration> endpoints)
    {
        // Register the endpoints if they don't exist
        RegisterEndpointConfigurations(endpoints);

        // We're done, return the instance
        return this;
    }

    /// <summary>
    /// This method registers a range of RabbitMQ endpoints
    /// </summary>
    /// <param name="endpoints">The RabbitMQ host and queue details</param>
    /// <returns>This instance</returns>
    public IQueueService RegisterEndpoints(params QueueConfiguration[] endpoints)
    {
        // Register the endpoints if they don't exist
        RegisterEndpointConfigurations(endpoints);

        // We're done, return the instance
        return this;
    }

    /// <summary>
    /// This method asynchronously subscribes to the queue
    /// </summary>
    /// <param name="delegateSubscriber">The message worker</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    /// <returns>An awaitable task containing the message</returns>
    public Task SubscribeAsync<TPayload>(IQueueService.DelegateSubscriberAsync<TPayload> delegateSubscriber,
        CancellationToken stoppingToken = default) =>
        new QueueSubscriber<TPayload>(_logger as ILogger<QueueSubscriber<TPayload>>, _queue?.GetChannel(), _queue?.Endpoint,
            _queue?.SimpleStorageService ?? _simpleStorageService).SubscribeAsync(delegateSubscriber, stoppingToken);

    /// <summary>
    /// This method asynchronously subscribes to <paramref name="queueName" />
    /// </summary>
    /// <param name="queueName">The queue to subscribe to</param>
    /// <param name="delegateSubscriber">The message worker</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    /// <returns>An awaitable task containing the message</returns>
    public Task SubscribeAsync<TPayload>(string queueName,
        IQueueService.DelegateSubscriberAsync<TPayload> delegateSubscriber,
        CancellationToken stoppingToken = default) =>
        WithDefaultEndpoint(queueName).SubscribeAsync(delegateSubscriber, stoppingToken);

    /// <summary>
    /// This method fluidly resets the queue's name into the instance
    /// </summary>
    /// <param name="queueName"></param>
    /// <returns></returns>
    public IQueueService WithDefaultEndpoint(string queueName) =>
        WithDefaultEndpoint(
            Queues.FirstOrDefault(q =>
                q.Endpoint.ToLower().Equals(queueName?.ToLower()) || q.Name.ToLower().Equals(queueName?.ToLower())),
            false);

    /// <summary>
    /// This method fluidly resets the default queue into the service
    /// </summary>
    /// <param name="endpoint">The queue configuration to use by default</param>
    /// <param name="register">Denotes whether to register the endpoint if it doesn't exist or not</param>
    /// <returns>This instance</returns>
    public IQueueService WithDefaultEndpoint(QueueConfiguration endpoint, bool register = true)
    {
        // Check the registration flag and register the endpoint
        if (register) RegisterEndpoint(endpoint);

        // Set the endpoint into the instance as the default queue
        _queue = endpoint;

        // We're done, return the instance
        return this;
    }

    /// <summary>
    /// This method provides a <paramref name="simpleStorageServiceConfiguration" /> for using AWS S3 in
    /// conjunction with RabbitMQ to reduce message size and durability
    /// </summary>
    /// <param name="simpleStorageServiceConfiguration">The AWS S3 configuration details</param>
    /// <returns></returns>
    public IQueueService WithSimpleStorageService(
        QueueSimpleStorageServiceConfiguration simpleStorageServiceConfiguration)
    {
        // Reset the simple storage service configuration into the instance
        _simpleStorageService = simpleStorageServiceConfiguration;

        // We're done, return the instance
        return this;
    }
}
