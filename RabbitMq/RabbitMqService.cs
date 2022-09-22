using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SyncStream.Serializer;

// Define our namespace
namespace SyncStream.Service.Queue.RabbitMq;

/// <summary>
/// This service is responsible for queueing things
/// </summary>
public class RabbitMqService : IRabbitMqService
{
    /// <summary>
    /// This property contains the list of available queues
    /// </summary>
    private static readonly List<RabbitMqQueueConfiguration> Queues = new();

    /// <summary>
    /// This method returns a queue endpoint configuration by it's name or endpoint
    /// </summary>
    /// <param name="queueName">The queue name or endpoint</param>
    /// <returns>The queue with the name or endpoint <paramref name="queueName" /> or null</returns>
    public static RabbitMqQueueConfiguration GetEndpointConfiguration(string queueName) =>
        Queues.FirstOrDefault(q =>
            q.Endpoint.ToLower().Equals(queueName.ToLower()) || q.Name.ToLower().Equals(queueName.ToLower()));

    /// <summary>
    /// This method registers a RabbitMQ endpoint configuration
    /// </summary>
    /// <param name="endpoint">The RabbitMQ host and queue details</param>
    /// <returns>This instance</returns>
    public static void RegisterEndpointConfiguration(RabbitMqQueueConfiguration endpoint)
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
    public static void RegisterEndpointConfigurations(IEnumerable<RabbitMqQueueConfiguration> endpoints) =>
        endpoints.ToList().ForEach(RegisterEndpointConfiguration);

    /// <summary>
    /// This method registers a range of RabbitMQ endpoints
    /// </summary>
    /// <param name="endpoints">The RabbitMQ host and queue details</param>
    /// <returns>This instance</returns>
    public static void RegisterEndpointConfigurations(params RabbitMqQueueConfiguration[] endpoints) =>
        RegisterEndpointConfigurations(endpoints.ToList());

    /// <summary>
    /// This property contains the instance of our default queue
    /// </summary>
    private RabbitMqQueueConfiguration _queue;

    /// <summary>
    /// This property contains the instance of our logger
    /// </summary>
    private readonly ILogger<RabbitMqService> _logger;

    /// <summary>
    /// This method instantiates our service with a RabbitMQ Connection
    /// </summary>
    /// <param name="logServiceProvider">The log service provider</param>
    /// <param name="defaultEndpoint">Optional default queue endpoint to use</param>
    public RabbitMqService(ILogger<RabbitMqService> logServiceProvider, RabbitMqQueueConfiguration defaultEndpoint = null)
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
    public RabbitMqService(ILogger<RabbitMqService> logServiceProvider, string defaultEndpoint = null)
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
    private RabbitMqMessage<TPayload> DeserializeMessage<TPayload>(ReadOnlyMemory<byte> message) =>
        JsonSerializer.Deserialize<RabbitMqMessage<TPayload>>(Encoding.UTF8.GetString(message.ToArray()));


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
    public RabbitMqMessage<TPayload> Publish<TPayload>(TPayload payload)
    {
        // Instantiate our message
        RabbitMqMessage<TPayload> message = new(payload);

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
    public RabbitMqMessage<TPayload> Publish<TPayload>(string queueName, TPayload payload) =>
        WithDefaultEndpoint(queueName).Publish(payload);

    /// <summary>
    /// This method registers a RabbitMQ endpoint
    /// </summary>
    /// <param name="endpoint">The RabbitMQ host and queue details</param>
    /// <returns>This instance</returns>
    public IRabbitMqService RegisterEndpoint(RabbitMqQueueConfiguration endpoint)
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
    public IRabbitMqService RegisterEndpoints(IEnumerable<RabbitMqQueueConfiguration> endpoints)
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
    public IRabbitMqService RegisterEndpoints(params RabbitMqQueueConfiguration[] endpoints)
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
    public void Subscribe<TPayload>(IRabbitMqService.DelegateSubscriber<TPayload> delegateSubscriber)
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
                RabbitMqMessage<TPayload> message = DeserializeMessage<TPayload>(eventArguments.Body);

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
        IRabbitMqService.DelegateSubscriber<TPayload> delegateSubscriber) =>
        WithDefaultEndpoint(queueName).Subscribe(delegateSubscriber);

    /// <summary>
    /// This method asynchronously subscribes to the queue
    /// </summary>
    /// <param name="delegateSubscriber">The message worker</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    /// <returns>An awaitable task containing the message</returns>
    public void Subscribe<TPayload>(IRabbitMqService.DelegateSubscriberAsync<TPayload> delegateSubscriber,
        CancellationToken stoppingToken = default)
    {
        // Localize the consumer
        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_queue?.GetChannel());

        // Attach our handler to the received list
        consumer.Received += async (_, eventArguments) =>
        {
            // Send the log message
            _logger.LogInformation("Incoming Message");

            // Try to deserialize the message and execute the subscriber
            try
            {
                // Deserialize the message
                RabbitMqMessage<TPayload> message = DeserializeMessage<TPayload>(eventArguments.Body);

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
        IRabbitMqService.DelegateSubscriberAsync<TPayload> delegateSubscriber,
        CancellationToken stoppingToken = default) =>
        WithDefaultEndpoint(queueName).Subscribe(delegateSubscriber, stoppingToken);

    /// <summary>
    /// This method fluidly resets the queue's name into the instance
    /// </summary>
    /// <param name="queueName"></param>
    /// <returns></returns>
    public IRabbitMqService WithDefaultEndpoint(string queueName) =>
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
    public IRabbitMqService WithDefaultEndpoint(RabbitMqQueueConfiguration endpoint, bool register = true)
    {
        // Check the registration flag and register the endpoint
        if (register) RegisterEndpoint(endpoint);

        // Set the endpoint into the instance as the default queue
        _queue = endpoint;

        // We're done, return the instance
        return this;
    }
}
