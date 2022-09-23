using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SyncStream.Aws.S3.Client;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
///
/// </summary>
/// <typeparam name="TPayload"></typeparam>
public abstract class QueuePublisherSubscriber<TPayload>
{
    /// <summary>
    /// This property contains our queue channel
    /// </summary>
   protected readonly IModel Channel;

    /// <summary>
    /// This property contains the log service provider for the instance
    /// </summary>
    protected readonly ILogger<QueuePublisherSubscriber<TPayload>> Logger;

    /// <summary>
    /// This property contains the queue endpoint for the subscriber
    /// </summary>
    protected readonly string Endpoint;

    /// <summary>
    /// This property contains the instance of our S3 configuration for AWS S3 backed queues
    /// </summary>
    protected readonly QueueSimpleStorageServiceConfiguration SimpleStorageServiceConfiguration;

    /// <summary>
    /// This method instantiates our publisher or subscriber"/>
    /// </summary>
    /// <param name="logServiceProvider">The log service provider for the instance</param>
    /// <param name="channel">The channel connection to the queue</param>
    /// <param name="endpoint">The queue endpoint</param>
    /// <param name="simpleStorageServiceConfiguration">Optional, AWS S3 configuration for alias messages</param>
    protected QueuePublisherSubscriber(ILogger<QueuePublisherSubscriber<TPayload>> logServiceProvider, IModel channel,
        string endpoint, QueueSimpleStorageServiceConfiguration simpleStorageServiceConfiguration = null)
    {
        // Set our queue channel into the instance
        Channel = channel;

        // Set our queue endpoint into the instance
        Endpoint = endpoint;

        // Set our log service provider into the instance
        Logger = logServiceProvider;

        // Set the AWS S3 configuration into the instance
        SimpleStorageServiceConfiguration = simpleStorageServiceConfiguration;
    }

    /// <summary>
    /// This method asynchronously downloads an alias message from AWS S3
    /// </summary>
    /// <param name="objectName">The object path of the alias message to download</param>
    /// <returns>An awaitable task containing the alias message</returns>
    public Task<SimpleStorageServiceQueueMessage<TPayload>>
        DownloadSimpleStorageServiceMessageAsync(string objectName) =>
        IsQueueBackedBySimpleStorageService()
            ? S3Client.DownloadObjectAsync<SimpleStorageServiceQueueMessage<TPayload>>($"{objectName}.json",
                configuration: SimpleStorageServiceConfiguration.ToClientConfiguration())
            : null;

    /// <summary>
    /// This method generates an AWS S3 object name for a message
    /// </summary>
    /// <param name="messageId">The unique ID of the queue message</param>
    /// <returns>The object name</returns>
    public string GenerateObjectName(Guid messageId) =>
        $"{SimpleStorageServiceConfiguration?.Bucket}/{Endpoint}/{messageId}";

    /// <summary>
    /// This method determines whether the queue is backed by AWS S3 or not
    /// </summary>
    /// <returns>Boolean denoting AWS S3 message aliasing</returns>
    public bool IsQueueBackedBySimpleStorageService() => SimpleStorageServiceConfiguration is not null;

    /// <summary>
    /// This method asynchronously writes the <paramref name="message" /> to
    /// AWS S3 at <paramref name="message.Payload" />.message.json
    /// </summary>
    /// <param name="message">The message to store</param>
    /// <returns>An awaitable task with a void result</returns>
    public Task WriteSimpleStorageServiceMessageAsync(SimpleStorageServiceQueueMessage<TPayload> message) =>
        IsQueueBackedBySimpleStorageService()
            ? S3Client.UploadAsync($"{message.Payload}.json", message,
                configuration: SimpleStorageServiceConfiguration.ToClientConfiguration())
            : Task.CompletedTask;
}
