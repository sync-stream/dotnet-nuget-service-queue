using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SyncStream.Aws.S3.Client;
using SyncStream.Aws.S3.Client.Config;
using SyncStream.Serializer;

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
    public QueueService(ILogger<QueueService> logServiceProvider,
        QueueConfiguration defaultEndpoint = null)
    {
        // Set the logger into the instance
        _logger = logServiceProvider;

        // Check for a default queue
        if (defaultEndpoint is not null) WithDefaultEndpoint(defaultEndpoint);
    }

    /// <summary>
    /// This method instantiates our service with a RabbitMQ Connection
    /// </summary>
    /// <param name="logServiceProvider">The log service provider</param>
    /// <param name="defaultEndpoint">Optional default queue endpoint to use</param>
    public QueueService(ILogger<QueueService> logServiceProvider, string defaultEndpoint = null)
    {
        // Set the logger into the instance
        _logger = logServiceProvider;

        // Check for a default queue
        if (defaultEndpoint is not null) WithDefaultEndpoint(defaultEndpoint);
    }

    /// <summary>
    /// This method deserializes a message from the queue
    /// </summary>
    /// <param name="message"></param>
    /// <typeparam name="TPayload"></typeparam>
    /// <returns></returns>
    private QueueMessage<TPayload> DeserializeMessage<TPayload>(ReadOnlyMemory<byte> message) =>
        JsonSerializer.Deserialize<QueueMessage<TPayload>>(Encoding.UTF8.GetString(message.ToArray()));

    /// <summary>
    /// This method generates an AWS S3 object name for a message
    /// </summary>
    /// <param name="messageId">The unique ID of the queue message</param>
    /// <returns>The object name</returns>
    private string GenerateObjectName(Guid messageId) =>
        $"{(_queue.SimpleStorageService ?? _simpleStorageService)?.Bucket}/{_queue.Endpoint}/{messageId}";

    /// <summary>
    /// This method asynchronously reads a message payload from S3
    /// </summary>
    /// <param name="message">The message containing the object path</param>
    /// <typeparam name="TPayload">The expected payload type</typeparam>
    /// <returns>An awaitable containing the downloaded payload</returns>
    private async Task<TPayload> ReadPayloadFromSimpleStorageService<TPayload>(QueueMessage<string> message)
    {
        // Define our client configuration
        S3ClientConfig clientConfig = new S3ClientConfig()
            .FromQueueServiceConfiguration(_queue.SimpleStorageService ?? _simpleStorageService);

        // Define our payload container
        object payload;

        // Check the type of the payload for a value type
        if (typeof(TPayload).IsValueType)
        {
            // Read the payload from S3
            Stream payloadStream = await S3Client.DownloadObjectAsync(message.Payload, clientConfig);

            // Define our reader
            using StreamReader reader = new(payloadStream);

            // Localize our payload
            payload = Convert.ChangeType(await reader.ReadToEndAsync(), typeof(TPayload));

        }

        // Otherwise, download the payload object from S3 and deserialize it
        else payload = await S3Client.DownloadObjectAsync<object>(message.Payload, configuration: clientConfig);

        // We're done, cast and return the downloaded S3 payload
        return (TPayload) payload;
    }

    /// <summary>
    /// This method asynchronously writes the <paramref name="message" /> to
    /// AWS S3 at <paramref name="message.Payload" />.message.json
    /// </summary>
    /// <param name="message">The message to store</param>
    /// <returns>An awaitable task with a void result</returns>
    private async Task WriteMessageToSimpleStorageService(QueueMessage<string> message)
    {
        // Check for an S3 configuration
        if (_queue.SimpleStorageService is not null || _simpleStorageService is not null)
        {
            // Localize our client configuration
            S3ClientConfig clientConfiguration = new S3ClientConfig()
                .FromQueueServiceConfiguration(_queue.SimpleStorageService ?? _simpleStorageService);

            // Upload the file to S3
            await S3Client.UploadAsync($"{message.Payload}.message.json", message,
                configuration: clientConfiguration);
        }
    }

    /// <summary>
    /// This method asynchronously writes the <paramref name="payload" /> to AWS S3 at <paramref name="objectName" />.payload.json
    /// </summary>
    /// <param name="objectName">The object name to store the JSON under</param>
    /// <param name="payload">The message to store</param>
    /// <returns>An awaitable task with a void result</returns>
    /// <typeparam name="TPayload">The expected type of <paramref name="payload"/></typeparam>
    private async Task WritePayloadToSimpleStorageService<TPayload>(string objectName, TPayload payload)
    {
        // Check for an S3 configuration
        if (_queue.SimpleStorageService is not null || _simpleStorageService is not null)
        {
            // Localize our client configuration
            S3ClientConfig clientConfiguration =
                new S3ClientConfig().FromQueueServiceConfiguration(_queue.SimpleStorageService ??
                                                                   _simpleStorageService);

            // Check for a reference type
            if (!typeof(TPayload).IsValueType)
                await S3Client.UploadAsync<object>($"{objectName}.payload.json", payload,
                    configuration: clientConfiguration);

            // Otherwise, upload the value type as a string
            else await S3Client.UploadAsync($"{objectName}.payload.json", payload?.ToString());
        }
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
    /// This method publishes a message to the queue
    /// </summary>
    /// <param name="payload"></param>
    /// <typeparam name="TPayload"></typeparam>
    /// <returns></returns>
    public QueueMessage<TPayload> Publish<TPayload>(TPayload payload)
    {
        // Instantiate our message
        QueueMessage<TPayload> message = new(payload);

        // Serialize the message
        string jsonMessage = JsonSerializer.Serialize(message);

        // Define our message body
        byte[] messageBody = Encoding.UTF8.GetBytes(jsonMessage);

        // Localize our properties
        IBasicProperties properties = _queue.GetChannel().CreateBasicProperties();

        // Set the content-type of the message
        properties.ContentType = "application/json";

        // Turn persistence on
        properties.DeliveryMode = 2;

        // Publish the message
        _queue.GetChannel().BasicPublish("", _queue.Endpoint, true, properties, messageBody);

        // Set the published timestamp into the new Queue Message
        message.Published = DateTime.UtcNow;

        // We're done, send the response
        return message;
    }

    /// <summary>
    /// This method publishes a message to the queue
    /// </summary>
    /// <param name="queueName"></param>
    /// <param name="payload"></param>
    /// <returns></returns>
    public QueueMessage<TPayload> Publish<TPayload>(string queueName, TPayload payload) =>
        WithDefaultEndpoint(queueName).Publish(payload);

    /// <summary>
    /// This method asynchronously publishes a message to the queue and optionally to S3
    /// </summary>
    /// <param name="payload">The content of the message to publish</param>
    /// <typeparam name="TPayload">The expected type of the message payload</typeparam>
    /// <returns>An awaitable task containing the published message</returns>
    public async Task<QueueMessage<TPayload>> PublishAsync<TPayload>(TPayload payload)
    {
        // Check for an S3 configuration
        if (_queue.SimpleStorageService is not null || _simpleStorageService is not null)
        {
            // Generate a new message ID
            Guid messageId = Guid.NewGuid();

            // Generate our new message
            QueueMessage<string> simpleStorageServiceMessage = Publish(GenerateObjectName(messageId));

            // Write the message to S3
            await WriteMessageToSimpleStorageService(simpleStorageServiceMessage);

            // Write the payload to S3
            await WritePayloadToSimpleStorageService(simpleStorageServiceMessage.Payload, payload);

            // We're done, return the message
            return new(payload)
            {
                // Set the unique ID into the response message
                Id = messageId,

                // Set the creation timestamp into the response message
                Created = simpleStorageServiceMessage.Created,

                // Set the published timestamp into the response message
                Published = simpleStorageServiceMessage.Published
            };
        }

        // We're done, publish and return the message
        return Publish(payload);
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
    /// This method subscribes to the queue
    /// </summary>
    /// <param name="delegateSubscriber">The subscription worker</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    public void Subscribe<TPayload>(IQueueService.DelegateSubscriber<TPayload> delegateSubscriber)
    {
        // Localize the consumer
        EventingBasicConsumer consumer = new EventingBasicConsumer(_queue?.GetChannel());

        // Attach our handler to the received list
        consumer.Received += (_, eventArguments) =>
        {

            // Send the log message
            _logger.LogDebug("Incoming Message");

            // Try to deserialize the message and execute the subscriber
            try
            {
                // Deserialize the message
                QueueMessage<TPayload> message = DeserializeMessage<TPayload>(eventArguments.Body);

                // Send the log message
                _logger.LogDebug(
                    $"Received Message (id: {message.Id}, published: {message.Published:O})");

                // Try to execute the subscriber
                try
                {
                    // Send the log message
                    _logger.LogDebug(
                        $"Invoking Subscriber (id: {message.Id}, published: {message.Published:O}), type: {delegateSubscriber.Method.DeclaringType?.Name ?? "Delegate"}.{delegateSubscriber.Method.Name}<RabbitMqMessage<{typeof(TPayload)}>>('{_queue?.Endpoint}')");

                    // Execute the subscriber
                    delegateSubscriber.Invoke(message);

                    // We're done, acknowledge the message
                    _queue?.GetChannel()?.BasicAck(eventArguments.DeliveryTag, false);

                    // Send the log message
                    _logger.LogDebug(
                        $"Acknowledged Message (id: {message.Id}, published: {message.Published:O}), type: {delegateSubscriber.Method.DeclaringType?.Name ?? "Delegate"}.{delegateSubscriber.Method.Name}<RabbitMqMessage<{typeof(TPayload)}>>('{_queue?.Endpoint}')");
                }
                catch (Exception subscriberException)
                {
                    // We're done, reject the message
                    _queue?.GetChannel()?.BasicReject(eventArguments.DeliveryTag, false);

                    // Send the log message
                    _logger.LogError(subscriberException,
                        $"Subscriber Rejected (id: {message.Id}, published: {message.Published:O}), type: {delegateSubscriber.Method.DeclaringType?.Name ?? "Delegate"}.{delegateSubscriber.Method.Name}<RabbitMqMessage<{typeof(TPayload)}>>('{_queue?.Endpoint}')");
                }
                finally
                {
                    // Reset the consumed timestamp on the message
                    message.Consumed = DateTime.UtcNow;
                }
            }
            catch (Exception exception)
            {
                // We're done, reject the message
                _queue?.GetChannel()?.BasicReject(eventArguments.DeliveryTag, false);

                // Send the log message
                _logger.LogError(exception, "Rejected");
            }
        };

        // Send the log message
        _logger.LogDebug($"Subscribed to {_queue?.Endpoint}");

        // Consume messages from the queue
        _queue?.GetChannel()?.BasicConsume(consumer, _queue?.Endpoint);
    }

    /// <summary>
    /// This method subscribes to <paramref name="queueName" />
    /// </summary>
    /// <param name="queueName">The queue to subscribe to</param>
    /// <param name="delegateSubscriber">The subscription worker</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    public void Subscribe<TPayload>(string queueName,
        IQueueService.DelegateSubscriber<TPayload> delegateSubscriber) =>
        WithDefaultEndpoint(queueName).Subscribe(delegateSubscriber);

    /// <summary>
    /// This method asynchronously subscribes to the queue
    /// </summary>
    /// <param name="delegateSubscriber">The message worker</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    /// <returns>An awaitable task containing the message</returns>
    public void Subscribe<TPayload>(IQueueService.DelegateSubscriberAsync<TPayload> delegateSubscriber,
        CancellationToken stoppingToken = default)
    {
        // Localize the consumer
        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_queue?.GetChannel());

        // Attach our handler to the received list
        consumer.Received += async (_, eventArguments) =>
        {
            // Throw an exception if the request has been cancelled
            stoppingToken.ThrowIfCancellationRequested();

            // Send the log message
            _logger.LogInformation("Incoming Message");

            // Try to deserialize the message and execute the subscriber
            try
            {
                // Deserialize the message
                QueueMessage<TPayload> message = DeserializeMessage<TPayload>(eventArguments.Body);

                // Send the log message
                _logger.LogDebug(
                    $"Received Message (id: {message.Id}, published: {message.Published:O})");

                // Try to execute the subscriber
                try
                {
                    // Send the log message
                    _logger.LogDebug(
                        $"Invoking Subscriber (id: {message.Id}, published: {message.Published:O}), type: {delegateSubscriber.Method.DeclaringType?.Name ?? "Delegate"}.{delegateSubscriber.Method.Name}<RabbitMqMessage<{typeof(TPayload)}>>('{_queue?.Endpoint}')");

                    // Check for an S3 queue message

                    // Execute the subscriber
                    await delegateSubscriber.Invoke(message, stoppingToken);

                    // We're done, acknowledge the message
                    _queue?.GetChannel()?.BasicAck(eventArguments.DeliveryTag, false);

                    // Send the log message
                    _logger.LogDebug(
                        $"Acknowledged Message (id: {message.Id}, published: {message.Published:O}), type: {delegateSubscriber.Method.DeclaringType?.Name ?? "Delegate"}.{delegateSubscriber.Method.Name}<RabbitMqMessage<{typeof(TPayload)}>>('{_queue?.Endpoint}')");
                }
                catch (Exception subscriberException)
                {
                    // We're done, reject the message
                    _queue?.GetChannel()?.BasicReject(eventArguments.DeliveryTag, false);

                    // Send the log message
                    _logger.LogError(subscriberException,
                        $"Subscriber Rejected (id: {message.Id}, published: {message.Published:O}), type: {delegateSubscriber.Method.DeclaringType?.Name ?? "Delegate"}.{delegateSubscriber.Method.Name}<RabbitMqMessage<{typeof(TPayload)}>>('{_queue?.Endpoint}')");
                }
                finally
                {
                    // Reset the consumed timestamp on the message
                    message.Consumed = DateTimeOffset.UtcNow.DateTime;
                }
            }
            catch (Exception exception)
            {
                // We're done, reject the message
                _queue?.GetChannel()?.BasicReject(eventArguments.DeliveryTag, false);

                // Send the log message
                _logger.LogError(exception, "Rejected");
            }
        };

        // Send the log message
        _logger.LogDebug($"Subscribed to {_queue?.Endpoint}");

        // Consume messages from the queue
        _queue?.GetChannel()?.BasicConsume(consumer, _queue?.Endpoint);
    }

    /// <summary>
    /// This method asynchronously subscribes to <paramref name="queueName" />
    /// </summary>
    /// <param name="queueName">The queue to subscribe to</param>
    /// <param name="delegateSubscriber">The message worker</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    /// <returns>An awaitable task containing the message</returns>
    public void Subscribe<TPayload>(string queueName,
        IQueueService.DelegateSubscriberAsync<TPayload> delegateSubscriber,
        CancellationToken stoppingToken = default) =>
        WithDefaultEndpoint(queueName).Subscribe(delegateSubscriber, stoppingToken);

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
