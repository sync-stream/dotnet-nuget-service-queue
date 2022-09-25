using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SyncStream.Cryptography;
using SyncStream.Serializer;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This class maintains the structure of our queue publisher
/// </summary>
/// <typeparam name="TPayload"></typeparam>
public class QueuePublisher<TPayload> : QueuePublisherSubscriber<TPayload>
{
    /// <summary>
    /// This method instantiates our queue publisher
    /// </summary>
    /// <param name="logServiceProvider">The log service provider for the instance</param>
    /// <param name="channel">The connection channel to the queue</param>
    /// <param name="endpoint">The queue endpoint we're connected to</param>
    /// <param name="simpleStorageServiceConfiguration">Optional, S3 storage configuration</param>
    /// <param name="encryptionConfiguration">The encryption configuration for the queue</param>
    public QueuePublisher(ILogger<QueuePublisher<TPayload>> logServiceProvider, IModel channel, string endpoint,
        QueueSimpleStorageServiceConfiguration simpleStorageServiceConfiguration = null,
        QueueServiceEncryptionConfiguration encryptionConfiguration = null) : base(logServiceProvider, channel,
        endpoint, simpleStorageServiceConfiguration, encryptionConfiguration)
    {
    }

    /// <summary>
    /// This method asynchronously publishes a message to the queue
    /// </summary>
    /// <param name="payload">The message payload</param>
    /// <returns>The message that was published</returns>
    public async Task<QueueMessage<TPayload>> PublishAsync(TPayload payload)
    {
        // Instantiate our message
        QueueMessage<TPayload> message = new(payload);

        // Serialize the message
        string json = EncryptionConfiguration is null
            ? JsonSerializer.Serialize(message)
            : await CryptographyService.EncryptAsync(message, key: EncryptionConfiguration.Secret,
                passes: EncryptionConfiguration.Passes);

        // Define our message body
        byte[] body = Encoding.UTF8.GetBytes(json);

        // Localize our properties
        IBasicProperties properties = Channel.CreateBasicProperties();

        // Set the content-type of the message
        properties.ContentType = "application/json";

        // Turn persistence on
        properties.DeliveryMode = 2;

        // Publish the message
        Channel.BasicPublish("", EndpointConfiguration, true, properties, body);

        // Set the published timestamp into the new Queue Message
        message.Published = DateTime.UtcNow;

        // Check for S3 capabilities
        if (IsQueueBackedBySimpleStorageService())
        {
            // Localize the S3 message
            SimpleStorageServiceQueueMessage<TPayload> simpleStorageServiceMessage =
                message.ToSimpleStorageServiceQueueMessage(GenerateObjectName(message.Id));

            // Commit the S3 message to storage
            await WriteSimpleStorageServiceMessageAsync(simpleStorageServiceMessage);

            // Set the S3 message into the message
            message.SimpleStorageServiceMessage = simpleStorageServiceMessage;
        }

        // We're done, send the response
        return message;
    }
}
