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
    ///     This method asynchronously publishes a message to the queue
    /// </summary>
    /// <param name="payload">The message payload</param>
    /// <returns>An awaitable task containing the message that was published</returns>
    public async Task<QueueMessage<TPayload>> PublishAsync(TPayload payload)
    {
        // Instantiate our message
        QueueMessage<TPayload> message = new(payload);

        // Define our message body
        byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(EndpointConfiguration.Encryption is null
            ? message
            : await message.ToEncryptedQueueMessageAsync(EndpointConfiguration.Encryption)));

        // Localize our properties
        IBasicProperties properties = EndpointConfiguration.GetChannel().CreateBasicProperties();

        // Set the content-type of the message
        properties.ContentType = "application/json";

        // Turn persistence on
        properties.DeliveryMode = 2;

        // Publish the message
        EndpointConfiguration.GetChannel().BasicPublish("", EndpointConfiguration.Endpoint, true, properties, body);

        // Set the published timestamp into the new Queue Message
        message.Published = DateTime.UtcNow;

        // Check for S3 capabilities
        if (!IsQueueBackedBySimpleStorageService()) return message;

        // Localize the S3 message
        SimpleStorageServiceQueueMessage<TPayload> simpleStorageServiceMessage =
            message.ToSimpleStorageServiceQueueMessage(GenerateObjectName(message.Id));

        // Commit the S3 message to storage
        await WriteSimpleStorageServiceMessageAsync(simpleStorageServiceMessage);

        // Set the S3 message into the message
        message.SimpleStorageServiceMessage = simpleStorageServiceMessage;

        // We're done, send the response
        return message;
    }
}
