using Microsoft.Extensions.Logging;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This service is responsible for queueing things
/// </summary>
public class QueueService : IQueueService
{
    /// <summary>
    /// This property contains the default cryptography settings to use
    /// </summary>
    public static QueueServiceEncryptionConfiguration DefaultEncryptionConfiguration { get; protected set; }

    /// <summary>
    /// This property contains the default queue to use
    /// </summary>
    public static QueueConfiguration DefaultEndpointConfiguration { get; protected set; }

    /// <summary>
    /// This property contains the default S3 configuration to use
    /// </summary>
    public static QueueSimpleStorageServiceConfiguration DefaultSimpleStorageServiceConfiguration
    {
        get;
        protected set;
    }

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
    /// This method registers the encryption <paramref name="configuration" /> as the global (default) for all queues
    /// </summary>
    /// <param name="configuration">The encryption configuration for the queue</param>
    /// <returns><paramref name="configuration" /></returns>
    public static QueueServiceEncryptionConfiguration RegisterDefaultEncryption(
        QueueServiceEncryptionConfiguration configuration)
    {
        // Register the queue encryption configuration
        DefaultEncryptionConfiguration = configuration;

        // We're done, return the default queue encryption configuration
        return DefaultEncryptionConfiguration;
    }

    /// <summary>
    /// This method fluidly resets the queue's name into the instance
    /// </summary>
    /// <param name="queueName"></param>
    /// <returns>The queue configuration that was recently set to default</returns>
    public static QueueConfiguration RegisterDefaultEndpoint(string queueName) =>
        RegisterDefaultEndpoint(
            Queues.FirstOrDefault(q =>
                q.Endpoint.ToLower().Equals(queueName?.ToLower()) || q.Name.ToLower().Equals(queueName?.ToLower())),
            false);

    /// <summary>
    /// This method registers the default endpoint for an existing queue instance
    /// </summary>
    /// <param name="instance">The current QueueService instance</param>
    /// <param name="queueName">The name of the queue endpoint to use</param>
    /// <returns><paramref name="instance" /></returns>
    public static IQueueService RegisterDefaultEndpoint(IQueueService instance, string queueName) =>
        instance.SetQueueEndpoint(RegisterDefaultEndpoint(queueName));

    /// <summary>
    /// This method fluidly resets the default queue into the service
    /// </summary>
    /// <param name="endpoint">The queue configuration to use by default</param>
    /// <param name="register">Denotes whether to register the endpoint if it doesn't exist or not</param>
    /// <returns>The queue configuration recently set as default</returns>
    public static QueueConfiguration RegisterDefaultEndpoint(QueueConfiguration endpoint, bool register = true)
    {
        // Check the registration flag and register the endpoint
        if (register) RegisterEndpointConfiguration(endpoint);

        // Set the default queue
        DefaultEndpointConfiguration = endpoint;

        // Check for encryption and register it
        if (endpoint.Encryption is not null) RegisterDefaultEncryption(endpoint.Encryption);

        // Check for an S3 configuration in the queue
        if (endpoint.SimpleStorageService is not null)
            RegisterDefaultSimpleStorageServiceConfiguration(endpoint.SimpleStorageService);

        // We're done, return the endpoint
        return DefaultEndpointConfiguration;
    }

    /// <summary>
    /// This method registers the default endpoint for an existing queue instance
    /// </summary>
    /// <param name="instance">The current QueueService instance</param>
    /// <param name="endpoint">The endpoint configuration to register</param>
    /// <param name="register">Optional, flag denoting whether to register the queue or not</param>
    /// <returns><paramref name="instance" /></returns>
    public static IQueueService RegisterDefaultEndpoint(IQueueService instance, QueueConfiguration endpoint,
        bool register = true) => instance.SetQueueEndpoint(RegisterDefaultEndpoint(endpoint, register));

    /// <summary>
    /// This method registers the default S3 configuration to use for any queue
    /// </summary>
    /// <param name="simpleStorageServiceConfiguration">The S3 configuration to register</param>
    /// <returns>The recently registered S3 configuration</returns>
    public static QueueSimpleStorageServiceConfiguration RegisterDefaultSimpleStorageServiceConfiguration(
        QueueSimpleStorageServiceConfiguration simpleStorageServiceConfiguration)
    {
        // Reset the default S3 configuration
        DefaultSimpleStorageServiceConfiguration = simpleStorageServiceConfiguration;

        // We're done, return the simple storage service configuration
        return DefaultSimpleStorageServiceConfiguration;
    }

    /// <summary>
    /// This method registers the default S3 configuration for an existing QueueService instance
    /// </summary>
    /// <param name="instance">The existing QueueService instance</param>
    /// <param name="simpleStorageServiceConfiguration">The S3 configuration to register</param>
    /// <returns><paramref name="instance" /></returns>
    public static IQueueService RegisterDefaultSimpleStorageServiceConfiguration(IQueueService instance,
        QueueSimpleStorageServiceConfiguration simpleStorageServiceConfiguration) =>
        instance.SetQueueSimpleStorageServiceConfiguration(
            RegisterDefaultSimpleStorageServiceConfiguration(simpleStorageServiceConfiguration));

    /// <summary>
    /// This method registers a RabbitMQ endpoint configuration
    /// </summary>
    /// <param name="endpoint">The RabbitMQ host and queue details</param>
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
    /// This property contains the encryption settings for the queue
    /// </summary>
    private QueueServiceEncryptionConfiguration _encryption;

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
    private QueueSimpleStorageServiceConfiguration _simpleStorageServiceConfiguration;

    /// <summary>
    /// This method instantiates our service with a RabbitMQ Connection
    /// </summary>
    /// <param name="logServiceProvider">The log service provider</param>
    public QueueService(ILogger<QueueService> logServiceProvider)
    {
        // Set the logger into the instance
        _logger = logServiceProvider;
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
    /// <returns>An awaitable task containing a void result</returns>
    public Task PublishAsync<TPayload>(TPayload payload) =>
        new QueuePublisher<TPayload>(_logger, _queue ?? DefaultEndpointConfiguration,
            _queue?.SimpleStorageService ??
            _simpleStorageServiceConfiguration ?? DefaultSimpleStorageServiceConfiguration,
            _queue?.Encryption ?? _encryption ?? DefaultEncryptionConfiguration).PublishAsync(payload);

    /// <summary>
    /// This method asynchronously publishes a message to <paramref name="queueName"/> and optionally to S3
    /// </summary>
    /// <param name="queueName">The queue to publish the message to</param>
    /// <param name="payload">The content of the message to publish</param>
    /// <typeparam name="TPayload">The expected type of the message payload</typeparam>
    /// <returns>An awaitable task containing the published message</returns>
    public Task PublishAsync<TPayload>(string queueName, TPayload payload) =>
        SetQueueEndpoint(GetEndpointConfiguration(queueName)).PublishAsync(payload);

    /// <summary>
    /// This method registers a RabbitMQ endpoint
    /// </summary>
    /// <param name="endpoint">The RabbitMQ host and queue details</param>
    /// <returns>This instance</returns>
    public IQueueService RegisterEndpoint(QueueConfiguration endpoint)
    {
        // Register the queue endpoint configuration
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
    /// This method fluidly resets the existing queue in the instance
    /// </summary>
    /// <param name="endpoint">The new queue endpoint to use by default</param>
    /// <returns>The current instance</returns>
    public IQueueService SetQueueEndpoint(QueueConfiguration endpoint)
    {
        // Reset the default queue endpoint into the instance
        _queue = endpoint;

        // We're done, return the instance
        return this;
    }

    /// <summary>
    /// This method fluidly resets the queue configuration into the instance
    /// </summary>
    /// <param name="configuration">The queue encryption configuration</param>
    /// <returns>The current instance</returns>
    public IQueueService SetQueueEncryptionConfiguration(QueueServiceEncryptionConfiguration configuration)
    {
        // Set the encryption into the instance
        _encryption = configuration;

        // We're done, return the instance
        return this;
    }

    /// <summary>
    /// This method fluidly resets the existing S3 configuration into the instance
    /// </summary>
    /// <param name="simpleStorageServiceConfiguration">The new S3 configuration to use by default</param>
    /// <returns>The current instance</returns>
    public IQueueService SetQueueSimpleStorageServiceConfiguration(
        QueueSimpleStorageServiceConfiguration simpleStorageServiceConfiguration)
    {
        // Reset the default queue S3 configuration into the instance
        _simpleStorageServiceConfiguration = simpleStorageServiceConfiguration;

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
        CancellationToken stoppingToken = default) => new QueueSubscriber<TPayload>(_logger,
        _queue ?? DefaultEndpointConfiguration,
        _simpleStorageServiceConfiguration ?? DefaultSimpleStorageServiceConfiguration,
        _encryption ?? DefaultEncryptionConfiguration).SubscribeAsync(delegateSubscriber, stoppingToken);

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
        SetQueueEndpoint(GetEndpointConfiguration(queueName)).SubscribeAsync(delegateSubscriber, stoppingToken);

    /// <summary>
    ///     This method fluidly resets the queue's encryption configuration into the instance
    /// </summary>
    /// <param name="encryptionConfiguration">The encryption configuration to use</param>
    /// <returns>The current instance</returns>
    public IQueueService UseEncryption(QueueServiceEncryptionConfiguration encryptionConfiguration)
    {
        // Reset the queue's encryption configuration into the instance
        _encryption = encryptionConfiguration;

        // Reset the queue's encryption configuration into the current queue endpoint configuration
        if (_queue is not null) _queue.Encryption ??= encryptionConfiguration;

        // We're done, return the instance
        return this;
    }

    /// <summary>
    ///     This method fluidly resets the queue endpoint into the instance
    /// </summary>
    /// <param name="queueEndpointConfiguration">The queue endpoint configuration to use</param>
    /// <returns>The current instance</returns>
    public IQueueService UseEndpoint(QueueConfiguration queueEndpointConfiguration)
    {
        // Reset the queue endpoint configuration into the instance
        _queue = queueEndpointConfiguration;

        // Check for an encryption configuration and use it
        if (_queue.Encryption is not null) _encryption = _queue.Encryption;

        // Check for an S3 configuration and use it
        if (_queue.SimpleStorageService is not null) _simpleStorageServiceConfiguration = _queue.SimpleStorageService;

        // We're done, return the instance
        return RegisterEndpoint(queueEndpointConfiguration);
    }

    /// <summary>
    ///     This method fluidly resets the queue endpoint into the instance
    /// </summary>
    /// <param name="queueEndpoint">The name of the queue endpoint configuration to use</param>
    /// <returns>The current instance</returns>
    public IQueueService UseEndpoint(string queueEndpoint) => UseEndpoint(Queues.FirstOrDefault(q =>
        q.Endpoint.ToLower().Equals(queueEndpoint.ToLower()) || q.Name.ToLower().Equals(queueEndpoint.ToLower())));

    /// <summary>
    ///     This method fluidly resets the queue's S3 configuration into the instance
    /// </summary>
    /// <param name="simpleStorageServiceConfiguration">The S3 configuration to use</param>
    /// <returns>The current instance</returns>
    public IQueueService UseSimpleStorageService(
        QueueSimpleStorageServiceConfiguration simpleStorageServiceConfiguration)
    {
        // Reset the queue's S3 configuration into the instance
        _simpleStorageServiceConfiguration = simpleStorageServiceConfiguration;

        // Reset the queue's S3 configuration into the current queue endpoint configuration
        if (_queue is not null) _queue.SimpleStorageService ??= simpleStorageServiceConfiguration;

        // We're done, return the instance
        return this;
    }
}
