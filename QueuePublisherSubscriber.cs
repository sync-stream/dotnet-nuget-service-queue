using Microsoft.Extensions.Logging;
using SyncStream.Aws.S3.Client;
using SyncStream.Serializer;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
///     This class maintains the structure of a publisher and subscriber
/// </summary>
/// <typeparam name="TPayload"></typeparam>
public abstract class QueuePublisherSubscriber<TPayload>
{
    /// <summary>
    ///     This property contains the log service provider for the instance
    /// </summary>
    protected readonly ILogger<IQueueService> Logger;

    /// <summary>
    ///     This property contains our queue endpoint configuration
    /// </summary>
    protected readonly QueueConfiguration EndpointConfiguration;

    /// <summary>
    ///     This method instantiates our publisher or subscriber
    /// </summary>
    /// <param name="logServiceProvider">The log service provider for the instance</param>
    /// <param name="endpointConfiguration">The queue endpoint configuration</param>
    /// <param name="simpleStorageServiceConfigurationOverride">Optional, AWS S3 configuration override for alias messages</param>
    /// <param name="encryptionConfigurationOverride">The encryption configuration override for the queue</param>
    protected QueuePublisherSubscriber(ILogger<IQueueService> logServiceProvider,
        QueueConfiguration endpointConfiguration,
        QueueSimpleStorageServiceConfiguration simpleStorageServiceConfigurationOverride = null,
        QueueServiceEncryptionConfiguration encryptionConfigurationOverride = null)
    {
        // Set our queue endpoint configuration into the instance
        EndpointConfiguration = endpointConfiguration;

        // Check for an encryption configuration override and use it
        if (encryptionConfigurationOverride is not null)
            EndpointConfiguration.Encryption = encryptionConfigurationOverride;

        // Check for an S3 configuration override and use it
        if (simpleStorageServiceConfigurationOverride is not null)
            EndpointConfiguration.SimpleStorageService = simpleStorageServiceConfigurationOverride;

        // Set our log service provider into the instance
        Logger = logServiceProvider;
    }

    /// <summary>
    ///     This method asynchronously downloads an alias message from AWS S3
    /// </summary>
    /// <param name="objectName">The object path of the alias message to download</param>
    /// <returns>An awaitable task containing the alias message</returns>
    public async Task<SimpleStorageServiceQueueMessage<TPayload>> DownloadSimpleStorageServiceMessageAsync(
        string objectName)
    {
        // Check for S3 capabilities
        if (!IsQueueBackedBySimpleStorageService()) return null;

        // Check for encryption
        if (EndpointConfiguration.Encryption is not null && EndpointConfiguration.SimpleStorageService.EncryptObjects)
        {
            // Download the message
            SimpleStorageServiceEncryptedQueueMessage<TPayload> encryptedMessage =
                await AwsSimpleStorageServiceClient
                    .DownloadObjectAsync<SimpleStorageServiceEncryptedQueueMessage<TPayload>>(objectName,
                        SerializerFormat.Json, EndpointConfiguration.SimpleStorageService.ToClientConfiguration());

            // We're done, return the decrypted message
            return await encryptedMessage.ToSimpleStorageServiceQueueMessageAsync(EndpointConfiguration.Encryption);
        }

        // We're done, download the object from S3 then return it
        return await AwsSimpleStorageServiceClient.DownloadObjectAsync<SimpleStorageServiceQueueMessage<TPayload>>(
            objectName,
            configuration: EndpointConfiguration.SimpleStorageService.ToClientConfiguration());
    }

    /// <summary>
    ///     This method generates an AWS S3 object name for a message
    /// </summary>
    /// <param name="messageId">The unique ID of the queue message</param>
    /// <returns>The object name</returns>
    public string GenerateObjectName(Guid messageId) =>
        $"{EndpointConfiguration.SimpleStorageService?.BucketPrefix}/{EndpointConfiguration}/{messageId}";

    /// <summary>
    ///     This method determines whether the queue is backed by AWS S3 or not
    /// </summary>
    /// <returns>Boolean denoting AWS S3 message aliasing</returns>
    public bool IsQueueBackedBySimpleStorageService() => EndpointConfiguration.SimpleStorageService is not null;

    /// <summary>
    ///     This method asynchronously writes the <paramref name="message" /> to
    ///     AWS S3 at <paramref name="message.Payload" />.message.json
    /// </summary>
    /// <param name="message">The message to store</param>
    /// <returns>An awaitable task with a void result</returns>
    public async Task WriteSimpleStorageServiceMessageAsync(SimpleStorageServiceQueueMessage<TPayload> message)
    {
        // Check for S3 capabilities
        if (!IsQueueBackedBySimpleStorageService()) return;

        // Check for encryption capabilities then encrypt the S3 JSON and upload it
        if (EndpointConfiguration.Encryption is not null && EndpointConfiguration.SimpleStorageService.EncryptObjects)
            await AwsSimpleStorageServiceClient.UploadAsync($"{message.Payload}.json",
                await message.ToSimpleStorageServiceEncryptedQueueMessageAsync(EndpointConfiguration.Encryption),
                format: SerializerFormat.Json,
                configuration: EndpointConfiguration.SimpleStorageService.ToClientConfiguration());

        // Otherwise, upload the message JSON
        else
            await AwsSimpleStorageServiceClient.UploadAsync($"{message.Payload}.json", message,
                format: SerializerFormat.Json,
                configuration: EndpointConfiguration.SimpleStorageService.ToClientConfiguration());
    }
}
