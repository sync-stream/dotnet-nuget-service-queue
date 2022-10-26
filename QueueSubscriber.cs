using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SyncStream.Serializer;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
///     This class maintains the structure of our queue subscriber
/// </summary>
/// <typeparam name="TPayload">The expected message type</typeparam>
public sealed class QueueSubscriber<TPayload> : QueuePublisherSubscriber<TPayload>
{
    /// <summary>
    ///     This property contains the instance of our consumer
    /// </summary>
    private readonly AsyncEventingBasicConsumer _consumer;

    /// <summary>
    ///     This method instantiates our subscriber with a <paramref name="logServiceProvider" />
    /// </summary>
    /// <param name="logServiceProvider">The log service provider for the instance</param>
    /// <param name="endpointConfiguration">The queue endpoint to subscribe to</param>
    /// <param name="simpleStorageServiceConfigurationOverride">Optional, AWS S3 configuration override details for S3-backed queue messages</param>
    /// <param name="encryptionConfigurationOverride">The encryption configuration override for the queue</param>
    public QueueSubscriber(ILogger<IQueueService> logServiceProvider, QueueConfiguration endpointConfiguration,
        QueueSimpleStorageServiceConfiguration simpleStorageServiceConfigurationOverride = null,
        QueueServiceEncryptionConfiguration encryptionConfigurationOverride = null) : base(logServiceProvider,
        endpointConfiguration, simpleStorageServiceConfigurationOverride, encryptionConfigurationOverride) =>
        _consumer = new(EndpointConfiguration.GetChannel());

    /// <summary>
    ///     This method asynchronously acknowledges an S3 alias message
    /// </summary>
    /// <param name="objectPath">The object path to the message on S3</param>
    private async Task AcknowledgeSimpleStorageServiceMessageAsync(string objectPath)
    {
        // Ensure we have an S3 configuration in the instance
        if (EndpointConfiguration.SimpleStorageService is null) return;

        // Send the log message
        GetLogger()?.LogInformation(GetLogMessage($"Acknowledging S3 Message at {objectPath}", null, null));

        // Download the alias message from S3
        SimpleStorageServiceQueueMessage<TPayload> message = await DownloadSimpleStorageServiceMessageAsync(objectPath);

        // Acknowledge the message
        message.Acknowledged = DateTimeOffset.UtcNow.DateTime;

        // Reset the consumed timestamp into the message
        message.Consumed = DateTimeOffset.UtcNow.DateTime;

        // Write the message back to S3
        await WriteSimpleStorageServiceMessageAsync(objectPath, message);
    }

    /// <summary>
    ///     This method asynchronously handles a message
    /// </summary>
    /// <param name="deliveryTag">The delivery tag of the message</param>
    /// <param name="delegateSubscriber">The asynchronous delegate subscriber</param>
    /// <param name="message">The message to send to the <paramref name="delegateSubscriber" /></param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <param name="objectName">Optional path to object in S3</param>
    /// <returns>An awaitable task containing a void result</returns>
    private async Task HandleMessageAsync(ulong deliveryTag,
        IQueueService.DelegateSubscriberAsync<TPayload> delegateSubscriber,
        QueueMessage<TPayload> message, CancellationToken stoppingToken = default, string objectName = null)
    {
        // Reset the consumed timestamp on the message
        message.Consumed = DateTimeOffset.UtcNow.DateTime;

        // Try to execute the subscriber
        try
        {
            // Send the log message
            GetLogger()?.LogInformation(GetLogMessage("Begin Consumer Invocation with Message", message,
                delegateSubscriber));

            // Execute the subscriber
            await delegateSubscriber.Invoke(message, stoppingToken);

            // Send the log message
            GetLogger()?.LogInformation(GetLogMessage("Consumer Invoked with Message", message, delegateSubscriber));

            // We're done, acknowledge the message
            EndpointConfiguration.GetChannel().BasicAck(deliveryTag, false);

            // Check for S3 aliasing and acknowledge the alias message
            if (EndpointConfiguration.SimpleStorageService is not null && objectName is not null)
                await AcknowledgeSimpleStorageServiceMessageAsync(objectName);

            // Send the log message
            GetLogger()?.LogInformation(GetLogMessage("Message Acknowledged by Consumer", message, delegateSubscriber));
        }
        catch (System.Exception subscriberException)
        {
            // Check for S3 aliasing and reject the alias message
            if (EndpointConfiguration.SimpleStorageService is not null && objectName is not null)
                await RejectSimpleStorageServiceMessageAsync(objectName, subscriberException);

            // We're done, reject the message
            EndpointConfiguration.GetChannel().BasicReject(deliveryTag, false);

            // Send the log message
            GetLogger()?.LogError(subscriberException,
                GetLogMessage($"Message Rejected by Consumer with {subscriberException.Message}", message,
                    delegateSubscriber));
        }
    }

    /// <summary>
    ///     This method asynchronously handles an incoming message to the subscriber
    /// </summary>
    /// <param name="arguments">The raw message from the queue</param>
    /// <param name="delegateSubscriber">The subscriber to pipe the pre-processed message to</param>
    /// <param name="stoppingToken">The token representing the cancellation of the task</param>
    /// <returns></returns>
    private async Task IncomingMessageAsync(BasicDeliverEventArgs arguments,
        IQueueService.DelegateSubscriberAsync<TPayload> delegateSubscriber, CancellationToken stoppingToken = default)
    {
        // Send the log message
        GetLogger()?.LogInformation(GetLogMessage("Incoming Message", null, delegateSubscriber));

        // Define our alias message
        QueueMessage<string> aliasMessage = null;

        // Try to deserialize the message and execute the subscriber
        try
        {
            // Define our message
            QueueMessage<TPayload> message;

            // Check for S3 then download and process the message
            if (EndpointConfiguration.SimpleStorageService is not null)
            {
                // Parse the alias message
                aliasMessage = await ParseIncomingMessageAsync<string>(arguments);

                // Download the alias message from S3
                SimpleStorageServiceQueueMessage<TPayload> bucketMessage =
                    await DownloadSimpleStorageServiceMessageAsync(aliasMessage.Payload);

                // Reset the message that we send to the subscriber
                message = bucketMessage.ToQueueMessage();
            }

            // Otherwise, parse the message normally
            else message = await ParseIncomingMessageAsync(arguments);

            // Send the log message
            GetLogger()?.LogInformation(GetLogMessage("Incoming Message Received", message, delegateSubscriber));

            // We're done execute our message handle and return its task
            await HandleMessageAsync(arguments.DeliveryTag, delegateSubscriber, message, stoppingToken,
                aliasMessage?.Payload);
        }

        // Catch any exceptions with the deserialization and processing
        catch (Exception exception)
        {
            // We're done, reject the message
            EndpointConfiguration.GetChannel().BasicReject(arguments.DeliveryTag, false);

            // Check for S3 aliasing and reject the S3 message
            if (EndpointConfiguration.SimpleStorageService is not null && aliasMessage?.Payload is not null)
                await RejectSimpleStorageServiceMessageAsync(aliasMessage.Payload, exception);

            // Send the log message
            GetLogger()?.LogError(exception,
                GetLogMessage($"Message Rejected with {exception.Message}", null, delegateSubscriber));
        }
    }

    /// <summary>
    ///     This method asynchronously parses an incoming message from the queue
    /// </summary>
    /// <param name="arguments">The incoming message event</param>
    /// <returns>An awaitable task containing the parsed message from <paramref name="arguments" /></returns>
    /// <typeparam name="TPayloadOverride">The expected type of the message</typeparam>
    private async Task<QueueMessage<TPayloadOverride>> ParseIncomingMessageAsync<TPayloadOverride>(
        BasicDeliverEventArgs arguments)
    {
        // Convert the message to a string
        string message = Encoding.UTF8.GetString(arguments.Body.ToArray());

        // Check for encryption
        if (EndpointConfiguration.Encryption is not null)
        {
            // Send the log message
            GetLogger()?.LogInformation(GetLogMessage("Deserializing Incoming Encrypted Message", null, null));

            // Localize the message
            EncryptedQueueMessage<TPayloadOverride> encryptedMessage =
                SerializerService.Deserialize<EncryptedQueueMessage<TPayloadOverride>>(message, EndpointConfiguration.SerializationFormat);

            // Send the log message
            GetLogger()?.LogInformation(GetLogMessage("Decrypting Incoming Message", null, null));

            // Decrypt and return the message
            return await encryptedMessage.ToQueueMessageAsync(EndpointConfiguration.Encryption);
        }

        // Send the log message
        GetLogger()?.LogInformation(GetLogMessage("Deserializing Incoming Message", null, null));

        // Deserialize and return the message
        return SerializerService.Deserialize<QueueMessage<TPayloadOverride>>(message, EndpointConfiguration.SerializationFormat);
    }

    /// <summary>
    ///     This method parses an incoming message from the queue
    /// </summary>
    /// <param name="arguments">The incoming message event</param>
    /// <returns>An awaitable task containing the parsed message from <paramref name="arguments" /></returns>
    private Task<QueueMessage<TPayload>> ParseIncomingMessageAsync(BasicDeliverEventArgs arguments) =>
        ParseIncomingMessageAsync<TPayload>(arguments);

    /// <summary>
    ///     This method asynchronously rejects an S3 alias message
    /// </summary>
    /// <param name="objectPath">The object path to the message on S3</param>
    /// <param name="reason">The reason the message was rejected</param>
    private async Task RejectSimpleStorageServiceMessageAsync(string objectPath, QueueMessageRejectedReason reason)
    {
        // Ensure we have an S3 configuration in the instance
        if (EndpointConfiguration.SimpleStorageService is null) return;

        // Send the log message
        GetLogger()?.LogError(GetLogMessage($"Rejected S3 Message at {objectPath} with {reason.Message}", null,
            null));

        // Download the alias message from S3
        SimpleStorageServiceQueueMessage<TPayload> message = await DownloadSimpleStorageServiceMessageAsync(objectPath);

        // Reset the consumed timestamp into the message
        message.Consumed = DateTimeOffset.UtcNow.DateTime;

        // Reject the message
        message.Rejected = DateTimeOffset.UtcNow.DateTime;

        // Set the reason into the message
        message.RejectedReason = reason;

        // Write the message back to S3
        await WriteSimpleStorageServiceMessageAsync(objectPath, message);
    }

    /// <summary>
    ///     This method provides an asynchronous consumer for queue messages
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
            if (stoppingToken.IsCancellationRequested)
            {
                // Send the log message
                GetLogger()?.LogInformation(GetLogMessage($"Consumer Cancelled with {stoppingToken.ToString()}", null,
                    delegateSubscriber));

                // We're done, return the completed task
                return Task.CompletedTask;
            }

            // We're done, return the awaiting message handler
            return IncomingMessageAsync(arguments, delegateSubscriber, stoppingToken);
        };

        // Consume messages from the queue
        _consumer.Model.BasicConsume(_consumer, EndpointConfiguration.Endpoint);

        // Send the log message
        GetLogger()?.LogInformation(GetLogMessage("Consumer Subscribed", null, delegateSubscriber));

        // We're done, return the completed task
        return Task.CompletedTask;
    }
}
