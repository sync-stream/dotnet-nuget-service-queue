using System.Text.RegularExpressions;
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
    private readonly ILogger<IQueueService> _logger;

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
        _logger = logServiceProvider;
    }

    /// <summary>
    /// This method generates the delegate type string from a <paramref name="delegateSubscriber" />
    /// </summary>
    /// <param name="delegateSubscriber">The subscriber delegate</param>
    /// <returns>The delegate type string for <paramref name="delegateSubscriber" /></returns>
    private string GetDelegateType(IQueueService.DelegateSubscriberAsync<TPayload> delegateSubscriber) =>
        string.Format("{0}{1}<QueueMessage<{2}>>",
            !string.IsNullOrEmpty(delegateSubscriber.Method.DeclaringType?.Name)
                ? $"{delegateSubscriber.Method.DeclaringType.Name}."
                : string.Empty,
            Regex.Replace(delegateSubscriber.Method.Name, @"^<(.*)>.*$", "$1", RegexOptions.IgnoreCase),
            typeof(TPayload).Name);

    /// <summary>
    ///     This method generates the publisher or subscriber message type
    /// </summary>
    /// <returns>The message type string</returns>
    private string GetMessageType() => string.Format("{0}<{1}>",
        EndpointConfiguration.Encryption is not null && EndpointConfiguration.SimpleStorageService is not null
            ? "SimpleStorageServiceEncryptedQueueMessage"
            : EndpointConfiguration.Encryption is not null
                ? "EncryptedQueueMessage"
                : EndpointConfiguration.SimpleStorageService is not null
                    ? "SimpleStorageServiceQueueMessage"
                    : "QueueMessage",
        typeof(TPayload).Name);

    /// <summary>
    ///     This method returns the logger from the instance if logging isn't suppresses
    /// </summary>
    /// <returns>The log service provider from the instance if logging isn't suppressed, otherwise <code>null</code></returns>
    protected ILogger<IQueueService> GetLogger() => EndpointConfiguration.SuppressLog ? null : _logger;

    /// <summary>
    ///     This method generates a log message for a publisher or subscriber
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <param name="queueMessage">Optional, queue message and payload</param>
    /// <param name="delegateSubscriber">Optional, subscriber delegate</param>
    /// <returns>The formatted and standardized log message</returns>
    protected string GetLogMessage(string message, QueueMessage<TPayload> queueMessage = null,
        IQueueService.DelegateSubscriberAsync<TPayload> delegateSubscriber = null)
    {
        // Define our log message header parts
        List<string> logMessageHeaders = new();

        // Check for a message and add the parts
        if (queueMessage is not null)
            logMessageHeaders.AddRange(new[] { $"id: {queueMessage.Id}", $"published: {queueMessage.Published:O}" });

        // Add the queue endpoint to the parts
        logMessageHeaders.Add($"endpoint: {EndpointConfiguration.Endpoint}");

        // Check for a delegate subscriber and add the parts
        if (delegateSubscriber is not null) logMessageHeaders.Add($"delegate: {GetDelegateType(delegateSubscriber)}");

        // Add the message type part
        logMessageHeaders.Add($"type: {GetMessageType()}");

        // We're done, return our message
        return $"[{DateTime.Now:O}]({string.Join(", ", logMessageHeaders)})" + (!string.IsNullOrEmpty(message) &&
            !string.IsNullOrWhiteSpace(message)
                ? $"\n\t{message}"
                : string.Empty);
    }

    /// <summary>
    ///     This method serializes a complex message for the publisher or subscriber then generates a log message
    /// </summary>
    /// <param name="message">The <typeparamref name="TLogMessage" /> object to serialize</param>
    /// <param name="queueMessage">Optional, queue message and payload</param>
    /// <param name="delegateSubscriber">Optional, subscriber delegate</param>
    /// <param name="format">Optional, serialization format to use</param>
    /// <returns>The formatted and standardized log message</returns>
    protected string GetLogMessage<TLogMessage>(TLogMessage message, QueueMessage<TPayload> queueMessage = null,
        IQueueService.DelegateSubscriberAsync<TPayload> delegateSubscriber = null,
        SerializerFormat format = SerializerFormat.Json) => GetLogMessage(
        $"\n{(format is SerializerFormat.Json ? JsonSerializer.SerializePretty(message) : XmlSerializer.SerializePretty(message))}\n\n",
        queueMessage, delegateSubscriber);

    /// <summary>
    ///     This method asynchronously downloads an alias message from AWS S3
    /// </summary>
    /// <param name="objectName">The object path of the alias message to download</param>
    /// <returns>An awaitable task containing the alias message</returns>
    protected async Task<SimpleStorageServiceQueueMessage<TPayload>> DownloadSimpleStorageServiceMessageAsync(
        string objectName)
    {
        // Check for S3 capabilities
        if (EndpointConfiguration.SimpleStorageService is null) return null;

        // Finalize our object name
        objectName = $"{objectName}.json";

        // Try to download the S3 message
        try
        {
            // Check for encryption
            if (EndpointConfiguration.Encryption is not null &&
                EndpointConfiguration.SimpleStorageService.EncryptObjects)
            {
                // Send the log message
                GetLogger()?.LogInformation(
                    GetLogMessage($"Downloading Encrypted S3 Message: {objectName}", null, null));

                // Download the message
                SimpleStorageServiceEncryptedQueueMessage<TPayload> encryptedMessage =
                    await AwsSimpleStorageServiceClient
                        .DownloadObjectAsync<SimpleStorageServiceEncryptedQueueMessage<TPayload>>(objectName,
                            EndpointConfiguration.SimpleStorageService.ToClientConfiguration());

                // Send the log message
                GetLogger()?.LogInformation(GetLogMessage($"Decrypting S3 Message: {objectName}", null, null));

                // We're done, return the decrypted message
                return await encryptedMessage.ToSimpleStorageServiceQueueMessageAsync(EndpointConfiguration.Encryption);
            }

            // Send the log message
            GetLogger()?.LogInformation(GetLogMessage($"Downloading S3 Message: {objectName}", null, null));

            // We're done, download the object from S3 then return it
            return await AwsSimpleStorageServiceClient.DownloadObjectAsync<SimpleStorageServiceQueueMessage<TPayload>>(
                objectName, EndpointConfiguration.SimpleStorageService.ToClientConfiguration());
        }

        catch (Exception exception)
        {
            // Send the log message
            GetLogger()?.LogError(exception,
                GetLogMessage(
                    $"Failed to Download S3 Message with {exception.InnerException?.Message ?? exception.Message}",
                    null, null));

            // We're done, return null
            return null;
        }
    }

    /// <summary>
    ///     This method generates an AWS S3 object name for a message
    /// </summary>
    /// <param name="messageId">The unique ID of the queue message</param>
    /// <returns>The object name</returns>
    protected string GenerateObjectPath(Guid messageId) => Regex.Replace($"{EndpointConfiguration.SimpleStorageService?.BucketPrefix}/{EndpointConfiguration?.Endpoint}/{DateTimeOffset.UtcNow.DateTime:yyyy/MM/dd}/{messageId}", @"/+", @"/");

    /// <summary>
    ///     This method asynchronously writes the <paramref name="message" /> to
    ///     AWS S3 at <paramref name="message.Payload" />.message.json
    /// </summary>
    /// <param name="objectPath">The path to the object in AWS S3</param>
    /// <param name="message">The message to store</param>
    /// <returns>An awaitable task with a void result</returns>
    protected async Task WriteSimpleStorageServiceMessageAsync(string objectPath, SimpleStorageServiceQueueMessage<TPayload> message)
    {
        // Check for S3 capabilities
        if (EndpointConfiguration.SimpleStorageService is null) return;

        // Try to write the message
        try
        {
            // Check for encryption capabilities
            if (EndpointConfiguration.Encryption is not null &&
                EndpointConfiguration.SimpleStorageService.EncryptObjects)
            {
                // Send the log message
                GetLogger()?.LogInformation(GetLogMessage($"Encrypting S3 Message: {objectPath}", null, null));

                // Encrypt the S3 message
                SimpleStorageServiceEncryptedQueueMessage<TPayload> encryptedMessage =
                    await message.ToSimpleStorageServiceEncryptedQueueMessageAsync(EndpointConfiguration.Encryption);

                // Send the lob message
                GetLogger()?.LogInformation(
                    GetLogMessage($"Serializing Encrypted S3 Message: {objectPath}", null, null));

                // Send the log message
                GetLogger()?.LogInformation(GetLogMessage($"Uploading Encrypted S3 Message: {objectPath}", null, null));

                // Upload the encrypted S3 message
                await AwsSimpleStorageServiceClient.UploadAsync(objectPath, encryptedMessage,
                    EndpointConfiguration.SimpleStorageService.ToClientConfiguration());
            }

            // Otherwise, upload the message JSON
            else
            {
                // Send the log message
                GetLogger()?.LogInformation(GetLogMessage($"Uploading S3 Message: {objectPath}", null, null));

                // Upload the S3 message
                await AwsSimpleStorageServiceClient.UploadAsync(objectPath, message,
                    EndpointConfiguration.SimpleStorageService.ToClientConfiguration());
            }
        }
        catch (Exception exception)
        {
            // Send the log message
            GetLogger()?.LogError(exception,
                GetLogMessage(
                    $"Failed to Write S3 Message {objectPath} with {exception?.InnerException?.Message ?? exception.Message}",
                    null,
                    null));
        }
    }
}
