using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This class maintains the structure of our queue subscriber
/// </summary>
/// <typeparam name="TPayload">The expected message type</typeparam>
public sealed class QueueSubscriber<TPayload> : QueuePublisherSubscriber<TPayload>
{
    /// <summary>
    /// This property contains the instance of our consumer
    /// </summary>
    private readonly AsyncEventingBasicConsumer _consumer;

    /// <summary>
    /// This method instantiates our subscriber with a <paramref name="logServiceProvider" />
    /// </summary>
    /// <param name="logServiceProvider">The log service provider for the instance</param>
    /// <param name="channel">The queue channel the subscriber is consuming</param>
    /// <param name="endpoint">The queue endpoint to subscribe to</param>
    /// <param name="simpleStorageServiceConfiguration">Optional, AWS S3 configuration details for S3-backed queue messages</param>
    /// <param name="encryptionConfiguration">The encryption configuration for the queue</param>
    public QueueSubscriber(ILogger<QueueSubscriber<TPayload>> logServiceProvider, IModel channel, string endpoint,
        QueueSimpleStorageServiceConfiguration simpleStorageServiceConfiguration = null,
        QueueServiceEncryptionConfiguration encryptionConfiguration = null) : base(logServiceProvider, channel,
        endpoint, simpleStorageServiceConfiguration, encryptionConfiguration)
    {
        // Set the consumer into the instance
        _consumer = new(Channel);
    }

    /// <summary>
    /// This method asynchronously acknowledges an S3 alias message
    /// </summary>
    /// <param name="objectName">The object path to the message on S3</param>
    public async Task AcknowledgeSimpleStorageServiceMessageAsync(string objectName)
    {
        // Ensure we have an S3 configuration in the instance
        if (!IsQueueBackedBySimpleStorageService()) return;

        // Download the alias message from S3
        SimpleStorageServiceQueueMessage<TPayload> message = await DownloadSimpleStorageServiceMessageAsync(objectName);

        // Acknowledge the message
        message.Acknowledged = DateTimeOffset.UtcNow.DateTime;

        // Write the message back to S3
        await WriteSimpleStorageServiceMessageAsync(message);
    }

    /// <summary>
    /// This method asynchronously handles a message
    /// </summary>
    /// <param name="deliveryTag">The delivery tag of the message</param>
    /// <param name="delegateSubscriber">The asynchronous delegate subscriber</param>
    /// <param name="message">The message to send to the <paramref name="delegateSubscriber" /></param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <param name="objectName">Optional path to object in S3</param>
    /// <returns>An awaitable task containing a void result</returns>
    public async Task HandleMessageAsync(ulong deliveryTag,
        IQueueService.DelegateSubscriberAsync<TPayload> delegateSubscriber,
        QueueMessage<TPayload> message, CancellationToken stoppingToken = default, string objectName = null)
    {
        // Reset the consumed timestamp on the message
        message.Consumed = DateTimeOffset.UtcNow.DateTime;

        // Try to execute the subscriber
        try
        {
            // Send the log message
            Logger?.LogDebug(
                "Message Consumer Invocation (id: {Id}, published: {Published}, alias: {Alias}, type: {Type}.{Name}<QueueMessage<{Payload}>>('{Endpoint}'))",
                message.Id, message.Published?.ToString("O"),
                objectName,
                delegateSubscriber.Method.DeclaringType?.Name ?? "Delegate", delegateSubscriber.Method.Name,
                typeof(TPayload), EndpointConfiguration);

            // Execute the subscriber
            await delegateSubscriber.Invoke(message, stoppingToken);

            // Send the log message
            Logger?.LogDebug(
                "Message Consumer Invoked (id: {Id}, published: {Published}, alias: {Alias}, type: {Type}.{Name}<QueueMessage<{Payload}>>('{Endpoint}'))",
                message.Id, message.Published?.ToString("O"),
                objectName,
                delegateSubscriber.Method.DeclaringType?.Name ?? "Delegate", delegateSubscriber.Method.Name,
                typeof(TPayload), EndpointConfiguration);

            // We're done, acknowledge the message
            Channel?.BasicAck(deliveryTag, false);

            // Check for S3 aliasing and acknowledge the alias message
            if (IsQueueBackedBySimpleStorageService() && objectName is not null)
                await AcknowledgeSimpleStorageServiceMessageAsync(objectName);

            // Send the log message
            Logger?.LogDebug(
                "Message Acknowledged (id: {Id}, published: {Published}, consumed: {Consumed}, alias: {Alias}, type: {Type}.{Name}<QueueMessage<{Payload}>>('{Endpoint}'))",
                message.Id, message.Published?.ToString("O"),
                message.Consumed?.ToString("O"),
                objectName,
                delegateSubscriber.Method.DeclaringType?.Name ?? "Delegate", delegateSubscriber.Method.Name,
                typeof(TPayload), EndpointConfiguration);
        }
        catch (Exception subscriberException)
        {
            // Check for S3 aliasing and reject the alias message
            if (IsQueueBackedBySimpleStorageService() && objectName is not null)
                await RejectSimpleStorageServiceMessageAsync(objectName, subscriberException);

            // We're done, reject the message
            Channel?.BasicReject(deliveryTag, false);

            // Send the log message
            Logger?.LogError(subscriberException,
                "Message Consumer Rejected (id: {Id}, published: {Published}, alias: {Alias}, type: {Type}.{Name}<QueueMessage<{Payload}>>('{Endpoint}'))",
                message.Id, message.Published?.ToString("O"),
                objectName,
                delegateSubscriber.Method.DeclaringType?.Name ?? "Delegate", delegateSubscriber.Method.Name,
                typeof(TPayload), EndpointConfiguration);
        }
    }

    /// <summary>
    /// This method asynchronously handles an incoming message to the subscriber
    /// </summary>
    /// <param name="arguments">The raw message from the queue</param>
    /// <param name="delegateSubscriber">The subscriber to pipe the pre-processed message to</param>
    /// <param name="stoppingToken">The token representing the cancellation of the task</param>
    /// <returns></returns>
    public async Task IncomingMessageAsync(BasicDeliverEventArgs arguments,
        IQueueService.DelegateSubscriberAsync<TPayload> delegateSubscriber, CancellationToken stoppingToken = default)
    {
        // Send the log message
        Logger?.LogInformation("Message Incoming");

        // Try to deserialize the message and execute the subscriber
        try
        {
            // Define our alias message
            QueueMessage<string> aliasMessage = null;

            // Define our message
            QueueMessage<TPayload> message = null;

            // Check for S3 then download and process the message
            if (IsQueueBackedBySimpleStorageService())
            {
                // Parse the alias message
                aliasMessage = ParseIncomingMessage<string>(arguments);

                // Download the alias message from S3
                SimpleStorageServiceQueueMessage<TPayload> bucketMessage =
                    await DownloadSimpleStorageServiceMessageAsync(aliasMessage.Payload);

                // Reset the message that we send to the subscriber
                message = bucketMessage.ToQueueMessage();
            }

            // Otherwise, parse the message normally
            else message = ParseIncomingMessage(arguments);

            // Send the log message
            Logger?.LogDebug("Message Received (id: {MessageId}, published: {MessagePublished})",
                message.Id, message.Published);

            // We're done execute our message handle and return its task
            await HandleMessageAsync(arguments.DeliveryTag, delegateSubscriber, message, stoppingToken,
                aliasMessage?.Payload);
        }

        // Catch any exceptions with the deserialization and processing
        catch (Exception exception)
        {
            // We're done, reject the message
            Channel?.BasicReject(arguments.DeliveryTag, false);

            // Send the log message
            Logger?.LogError(exception, "Message Rejected");
        }
    }

    /// <summary>
    /// This method parses an incoming message from the queue
    /// </summary>
    /// <param name="arguments">The incoming message event</param>
    /// <returns>The parsed message from <paramref name="arguments" /></returns>
    public QueueMessage<TPayload> ParseIncomingMessage(BasicDeliverEventArgs arguments) =>
        JsonSerializer.Deserialize<QueueMessage<TPayload>>(Encoding.UTF8.GetString(arguments.Body.ToArray()));

    /// <summary>
    /// This method parses an incoming message from the queue
    /// </summary>
    /// <param name="arguments">The incoming message event</param>
    /// <returns>The parsed message from <paramref name="arguments" /></returns>
    /// <typeparam name="TPayloadOverride">The expected type of the message</typeparam>
    public QueueMessage<TPayloadOverride> ParseIncomingMessage<TPayloadOverride>(BasicDeliverEventArgs arguments) =>
        JsonSerializer.Deserialize<QueueMessage<TPayloadOverride>>(Encoding.UTF8.GetString(arguments.Body.ToArray()));

    /// <summary>
    /// This method asynchronously rejects an S3 alias message
    /// </summary>
    /// <param name="objectName">The object path to the message on S3</param>
    /// <param name="reason">The reason the message was rejected</param>
    public async Task RejectSimpleStorageServiceMessageAsync(string objectName, QueueMessageRejectedReason reason)
    {
        // Ensure we have an S3 configuration in the instance
        if (!IsQueueBackedBySimpleStorageService()) return;

        // Download the alias message from S3
        SimpleStorageServiceQueueMessage<TPayload> message = await DownloadSimpleStorageServiceMessageAsync(objectName);

        // Reject the message
        message.Rejected = DateTimeOffset.UtcNow.DateTime;

        // Set the reason into the message
        message.RejectedReason = reason;

        // Write the message back to S3
        await WriteSimpleStorageServiceMessageAsync(message);
    }

    /// <summary>
    /// This method provides an asynchronous consumer for queue messages
    /// </summary>
    /// <param name="delegateSubscriber">The asynchronous subscriber delegate to run after pre-processing the queue message</param>
    /// <param name="stoppingToken">The token representing the cancellation of the task</param>
    /// <returns>An awaitable task with a void result</returns>
    public Task SubscribeAsync(IQueueService.DelegateSubscriberAsync<TPayload> delegateSubscriber,
        CancellationToken stoppingToken = default)
    {
        // Bind to the consumer's receiver
        _consumer.Received += (_, arguments) =>
        {
            // Halt execution if the cancellation token has been requested
            if (stoppingToken.IsCancellationRequested) return Task.CompletedTask;

            // We're done, return the awaiting message handler
            return IncomingMessageAsync(arguments, delegateSubscriber, stoppingToken);
        };

        // Send the log message
        Logger?.LogDebug("Consuming {Endpoint}", EndpointConfiguration);

        // Consume messages from the queue
        Channel?.BasicConsume(_consumer, EndpointConfiguration);

        // We're done, return the completed task
        return Task.CompletedTask;
    }
}
