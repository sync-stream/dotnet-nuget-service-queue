using System.Text.Json.Serialization;
using System.Xml.Serialization;
using SyncStream.Cryptography;
using SyncStream.Serializer;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
///     This class maintains the structure of an S3 alias queue message
/// </summary>
/// <typeparam name="TEnvelope">The expected type of the envelope</typeparam>
[XmlInclude(typeof(QueueMessageRejectedReason))]
[XmlRoot("simpleStorageServiceQueueMessage")]
public class SimpleStorageServiceQueueMessage<TEnvelope> : QueueMessage<string>,
    ISimpleStorageServiceQueueMessage<TEnvelope>
{
    /// <summary>
    ///     This property contains the timestamp at which the queue message was acknowledged
    /// </summary>
    [JsonPropertyName("acknowledged")]
    [XmlElement("acknowledged")]
    public DateTime? Acknowledged { get; set; }

    /// <summary>
    /// This property contains the payload for the Queue Message
    /// </summary>
    [JsonPropertyName("payload")]
    [XmlElement("payload")]
    public new string Payload { get; set; }

    /// <summary>
    ///     This property contains the original payload for the message
    /// </summary>
    [JsonPropertyName("envelope")]
    [XmlElement("envelope")]
    public TEnvelope Envelope { get; set; }

    /// <summary>
    ///     This property contains the reason as to why the message was rejected
    /// </summary>
    [JsonPropertyName("rejectedReason")]
    [XmlElement("rejectedReason")]
    public QueueMessageRejectedReason RejectedReason { get; set; }

    /// <summary>
    ///     This method instantiates a new message
    /// </summary>
    public SimpleStorageServiceQueueMessage()
    {
    }

    /// <summary>
    ///     This method instantiates the message with a <paramref name="payload" />
    /// </summary>
    /// <param name="payload">The payload for the message</param>
    public SimpleStorageServiceQueueMessage(string payload) : base(payload)
    {
    }

    /// <summary>
    ///     This method converts the instance to an encrypted S3 queue message object with
    ///     optional encrypted <paramref name="payload" /> and/or <paramref name="envelope" />
    /// </summary>
    /// <param name="payload">Optional, encrypted payload</param>
    /// <param name="envelope">Optional, encrypted envelope</param>
    /// <returns>The current instance converted to an encrypted s3 Queue message object without a payload or envelope</returns>
    public SimpleStorageServiceEncryptedQueueMessage<TEnvelope> ToSimpleStorageServiceEncryptedQueueMessage(
        string payload = null, string envelope = null)
    {
        // Define our response
        SimpleStorageServiceEncryptedQueueMessage<TEnvelope> response = new()
        {
            // Set the acknowledged timestamp into the response
            Acknowledged = Acknowledged,

            // Set the consumed timestamp into the response
            Consumed = Consumed,

            // St the creation timestamp into the response
            Created = Created,

            // Set the unique ID into the response
            Id = Id,

            // Set the published timestamp into the response
            Published = Published,

            // Set the rejection timestamp into the response
            Rejected = Rejected,

            // Set the rejection reason into the response
            RejectedReason = RejectedReason
        };

        // Check for a provided envelope and use it
        if (envelope is not null) response.Envelope = envelope;

        // Check for a provided payload and use it
        if (payload is not null) response.Payload = payload;

        // We're done, send the response
        return response;
    }

    /// <summary>
    ///     This method converts the current instance into an encrypted S3 queue message object
    /// </summary>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <returns>The encrypted S3 queue message</returns>
    public SimpleStorageServiceEncryptedQueueMessage<TEnvelope>
        ToSimpleStorageServiceEncryptedQueueMessage(QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        ToSimpleStorageServiceEncryptedQueueMessage().WithEnvelope(Envelope, encryptionConfiguration)
            .WithPayload(Payload, encryptionConfiguration);

    /// <summary>
    ///     This method converts the current instance into an encrypted S3 queue message object
    /// </summary>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <returns>The encrypted S3 queue message</returns>
    public async Task<SimpleStorageServiceEncryptedQueueMessage<TEnvelope>>
        ToSimpleStorageServiceEncryptedQueueMessageAsync(QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        await (await ToSimpleStorageServiceEncryptedQueueMessage().WithEnvelopeAsync(Envelope, encryptionConfiguration))
            .WithPayloadAsync(Payload, encryptionConfiguration);

    /// <summary>
    ///     This method converts the instance to an alias queue message
    /// </summary>
    /// <returns>The queue alias message</returns>
    public QueueMessage<string> ToQueueAliasMessage() => new()
    {
        // Set the consumed timestamp into the response
        Consumed = Consumed,

        // Set the creation timestamp into the response
        Created = Created,

        // Set the unique message ID into the response
        Id = Id,

        // Set the payload envelope into the response
        Payload = Payload,

        // Set the published timestamp into the response
        Published = Published,

        // Set the rejected timestamp into the response
        Rejected = Rejected
    };

    /// <summary>
    ///     This method generates an appropriately typed queue message from the instance
    /// </summary>
    /// <returns>A basic, typed QueueMessage</returns>
    public QueueMessage<TEnvelope> ToQueueMessage() => new()
    {
        // Set the consumed timestamp into the response
        Consumed = Consumed,

        // Set the creation timestamp into the response
        Created = Created,

        // Set the unique message ID into the response
        Id = Id,

        // Set the payload envelope into the response
        Payload = Envelope,

        // Set the published timestamp into the response
        Published = Published,

        // Set the rejected timestamp into the response
        Rejected = Rejected,

        // Set the S3 message into the instance
        SimpleStorageServiceMessage = this
    };
}
