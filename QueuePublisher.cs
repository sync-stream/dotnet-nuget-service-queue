using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SyncStream.Serializer;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
///     This class maintains the structure of our queue publisher
/// </summary>
/// <typeparam name="TPayload"></typeparam>
public class QueuePublisher<TPayload> : QueuePublisherSubscriber<TPayload>
{
    /// <summary>
    ///     This method instantiates our queue publisher
    /// </summary>
    /// <param name="logServiceProvider">The log service provider for the instance</param>
    /// <param name="endpointConfiguration">The queue endpoint we're connected to</param>
    /// <param name="simpleStorageServiceConfigurationOverride">Optional, S3 storage configuration override</param>
    /// <param name="encryptionConfigurationOverride">Optional, encryption configuration override for the queue</param>
    public QueuePublisher(ILogger<IQueueService> logServiceProvider, QueueConfiguration endpointConfiguration,
        QueueSimpleStorageServiceConfiguration simpleStorageServiceConfigurationOverride = null,
        QueueServiceEncryptionConfiguration encryptionConfigurationOverride = null) : base(logServiceProvider,
        endpointConfiguration, simpleStorageServiceConfigurationOverride, encryptionConfigurationOverride)
    {
    }

    /// <summary>
    ///     This method asynchronously serializes and publishes a queue message
    /// </summary>
    /// <param name="payload">The payload to serialize</param>
    /// <returns>The serialized message</returns>
    private async Task<string> PublishMessageAsync(TPayload payload)
    {
        // Instantiate our message
        QueueMessage<TPayload> message = new(payload) { Published = DateTimeOffset.UtcNow.DateTime };

        // Define our serialization container
        string serialized;

        // Check for encryption then encrypt the message
        if (EndpointConfiguration.Encryption is not null)
        {
            // Send the log message
            GetLogger()?.LogInformation(GetLogMessage("Encrypting New Message", null, null));

            // Encrypt and serialize the message
            serialized =
                JsonSerializer.Serialize(await message.ToEncryptedQueueMessageAsync(EndpointConfiguration.Encryption));
        }

        // Otherwise, serialize the message
        else serialized = JsonSerializer.Serialize(message);

        // We're done, return the serialized object
        return serialized;
    }

    /// <summary>
    /// This method serializes an object for the queue with S3 aliasing and optional encryption
    /// </summary>
    /// <param name="payload">The payload to send to the queue</param>
    /// <returns>The serialized message</returns>
    private async Task<string> PublishSimpleStorageServiceMessageAsync(TPayload payload)
    {
        // Instantiate our message
        QueueMessage<TPayload> message = new(payload) { Published = DateTimeOffset.UtcNow.DateTime };

        // Convert our message to an S3 message
        SimpleStorageServiceQueueMessage<TPayload> simpleStorageServiceMessage =
            message.ToSimpleStorageServiceQueueMessage(GenerateObjectName(message.Id));

        // Write the message to S3
        await WriteSimpleStorageServiceMessageAsync(simpleStorageServiceMessage);

        // Define our serialization container
        string serialized;

        // Check for encryption then encrypt the message
        if (EndpointConfiguration.Encryption is not null)
        {
            // Send the log message
            GetLogger()?.LogInformation(GetLogMessage("Generating and Encrypting New Alias Message", null, null));

            // Encrypt and serialize the alias message
            serialized = JsonSerializer.Serialize(await simpleStorageServiceMessage.ToQueueAliasMessage()
                .ToEncryptedQueueMessageAsync(EndpointConfiguration.Encryption));
        }

        // Otherwise, convert the message to an S3 message and serialize it
        else
        {
            // Send the log message
            GetLogger()?.LogInformation(GetLogMessage("Generating New Alias Message", null, null));

            // Serialize the alias message
            serialized =
                JsonSerializer.Serialize(simpleStorageServiceMessage.ToQueueAliasMessage());
        }

        // We're done, return the serialized object
        return serialized;
    }

    /// <summary>
    ///     This method asynchronously publishes a message to the queue
    /// </summary>
    /// <param name="payload">The message payload</param>
    /// <returns>An awaitable task containing a void result</returns>
    public async Task PublishAsync(TPayload payload)
    {
        // Send the log message
        GetLogger()?.LogInformation(GetLogMessage("Generating New Message", null, null));

        // Define our serialized string
        string serialized;

        // Check for encryption
        if (EndpointConfiguration.SimpleStorageService is not null)
            serialized = await PublishSimpleStorageServiceMessageAsync(payload);

        // Otherwise, send the log message and serialize the message
        else
            serialized = await PublishMessageAsync(payload);

        // Define our message body
        byte[] body = Encoding.UTF8.GetBytes(serialized);

        // Localize our properties
        IBasicProperties properties = EndpointConfiguration.GetChannel().CreateBasicProperties();

        // Set the content-type of the message
        properties.ContentType = "application/json";

        // Turn persistence on
        properties.DeliveryMode = 2;

        // Publish the message
        EndpointConfiguration.GetChannel().BasicPublish("", EndpointConfiguration.Endpoint, true, properties, body);

        // Send the log message
        GetLogger()?.LogInformation(GetLogMessage(
            $"Published New {(EndpointConfiguration.Encryption is not null ? "Encrypted " : string.Empty)} Message",
            null, null));
    }
}
