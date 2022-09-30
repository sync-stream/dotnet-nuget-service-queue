using System.Text.Json.Serialization;
using System.Xml.Serialization;
using SyncStream.Cryptography;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
///     This class maintains the structure of our encrypt S3 queue message
/// </summary>
/// <typeparam name="TEnvelope">The expected message type</typeparam>
[XmlRoot("encryptedSimpleStorageServiceQueueMessage")]
public class SimpleStorageServiceEncryptedQueueMessage<TEnvelope> : EncryptedQueueMessage<TEnvelope>,
    ISimpleStorageServiceQueueMessage<string>
{
    /// <summary>
    ///     This property contains the timestamp at which the queue message was acknowledged
    /// </summary>
    [JsonPropertyName("acknowledged")]
    [XmlElement("acknowledged")]
    public DateTime? Acknowledged { get; set; }

    /// <summary>
    ///     This property contains the original payload for the message
    /// </summary>
    [JsonPropertyName("envelope")]
    [XmlElement("envelope")]
    public string Envelope { get; set; }

    /// <summary>
    ///     This property contains the timestamp at which the message was rejected
    /// </summary>
    [JsonPropertyName("rejected")]
    [XmlElement("rejected")]
    public DateTime? Rejected { get; set; }

    /// <summary>
    ///     This property contains the reason as to why the message was rejected
    /// </summary>
    [JsonPropertyName("rejectedReason")]
    [XmlElement("rejectedReason")]
    public QueueMessageRejectedReason RejectedReason { get; set; }

    /// <summary>
    ///     This method instantiates an empty encrypted S3 queue message
    /// </summary>
    public SimpleStorageServiceEncryptedQueueMessage()
    {
    }

    /// <summary>
    ///     This method instantiates an encrypted S3 queue message from a <paramref name="payload" />
    /// </summary>
    /// <param name="payload">The payload to encrypt</param>
    /// <param name="encryptionConfiguration">The queue encryption configuration</param>
    public SimpleStorageServiceEncryptedQueueMessage(string payload,
        QueueServiceEncryptionConfiguration encryptionConfiguration) : base(payload, encryptionConfiguration)
    {
    }

    /// <summary>
    /// This method instantiates an encrypted message with an encrypted <paramref name="payload" /> and plain <paramref name="envelope" />
    /// </summary>
    /// <param name="payload">The encrypted payload</param>
    /// <param name="envelope">The plain envelope object</param>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    public SimpleStorageServiceEncryptedQueueMessage(string payload, TEnvelope envelope,
        QueueServiceEncryptionConfiguration encryptionConfiguration) : this(payload, encryptionConfiguration) =>
        WithEnvelope(envelope, encryptionConfiguration);

    /// <summary>
    ///     This method instantiates an encrypted message from an existing <paramref name="message" />
    /// </summary>
    /// <param name="message">The existing S3 queue message</param>
    /// <param name="encryptionConfiguration">The queue encryption configuration </param>
    public SimpleStorageServiceEncryptedQueueMessage(SimpleStorageServiceQueueMessage<TEnvelope> message,
        QueueServiceEncryptionConfiguration encryptionConfiguration)
    {
        // Set the acknowledged timestamp into the instance
        Acknowledged = message.Acknowledged;

        // Set the consumed timestamp into the instance
        Consumed = message.Consumed;

        // Set the created timestamp into the instance
        Created = message.Created;

        // Set the unique ID of the message into the instance
        Id = message.Id;

        // Encrypt the payload and set the hash into the instance
        Payload = Encrypt(message.Payload, encryptionConfiguration);

        // Set the published timestamp into the instance
        Published = message.Published;

        // Set the rejection timestamp into the instance
        Rejected = message.Rejected;

        // Set the rejected reason into the instance
        RejectedReason = message.RejectedReason;

        // Set the envelope into the message
        WithEnvelope(message.Envelope, encryptionConfiguration);

        // Set the payload into the instance
        WithPayload(message.Payload, encryptionConfiguration);
    }

    /// <summary>
    ///     This method decrypts and returns the envelope from the instance
    /// </summary>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <returns>The decrypted envelope from the instance</returns>
    public TEnvelope GetEnvelope(QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        Decrypt<TEnvelope>(Envelope, encryptionConfiguration);

    /// <summary>
    ///     This method asynchronously decrypts and returns the envelope from the instance
    /// </summary>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <returns>An awaitable task containing the decrypted envelope from the instance</returns>
    public Task<TEnvelope> GetEnvelopeAsync(QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        DecryptAsync<TEnvelope>(Envelope, encryptionConfiguration);

    /// <summary>
    ///     This method decrypts and returns the payload from the instance
    /// </summary>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <returns>The decrypted payload from the instance</returns>
    public new string GetPayload(QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        Decrypt(Payload, encryptionConfiguration);

    /// <summary>
    ///     This method asynchronously decrypts and returns the payload from the instance
    /// </summary>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <returns>An awaitable task containing the decrypted payload from the instance</returns>
    public new Task<string> GetPayloadAsync(QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        DecryptAsync(Payload, encryptionConfiguration);

    /// <summary>
    ///     This method converts the instance to a decrypted queue message object
    /// </summary>
    /// <param name="envelope">The decrypted envelope from the instance</param>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <param name="message">Optional, decrypted S3 queue message</param>
    /// <returns>The decrypted queue message</returns>
    public QueueMessage<TEnvelope> ToQueueMessage(TEnvelope envelope,
        QueueServiceEncryptionConfiguration encryptionConfiguration,
        SimpleStorageServiceQueueMessage<TEnvelope> message = null) => new()
    {
        // Set the consumed timestamp into the response
        Consumed = Consumed,

        // Set the creation timestamp into the response
        Created = Created,

        // Set the unique ID into the response
        Id = Id,

        // Set the decrypted payload into the response
        Payload = GetEnvelope(encryptionConfiguration),

        // Set the published timestamp into the response
        Published = Published,

        // Set the S3 message into the response
        SimpleStorageServiceMessage = message ?? ToSimpleStorageServiceQueueMessage(encryptionConfiguration)
    };

    /// <summary>
    ///     This method decrypts the payload and envelope then generates a queue message object
    /// </summary>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <returns>The decrypted queue message object</returns>
    public new QueueMessage<TEnvelope> ToQueueMessage(QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        ToQueueMessage(GetEnvelope(encryptionConfiguration), encryptionConfiguration,
            ToSimpleStorageServiceQueueMessage(encryptionConfiguration));

    /// <summary>
    ///     This method asynchronously decrypts the payload and envelope then generates a queue message object
    /// </summary>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <returns>An awaitable task containing the decrypted queue message object</returns>
    public new async Task<QueueMessage<TEnvelope>>
        ToQueueMessageAsync(QueueServiceEncryptionConfiguration encryptionConfiguration) => ToQueueMessage(
        await GetEnvelopeAsync(encryptionConfiguration), encryptionConfiguration,
        await ToSimpleStorageServiceQueueMessageAsync(encryptionConfiguration));

    /// <summary>
    ///     This method converts the instance to a decrypted S3 queue message object
    /// </summary>
    /// <param name="envelope">The decrypted envelope from the instance</param>
    /// <param name="payload">The decrypted payload (S3 object key) from the instance</param>
    /// <returns>The decrypted S3 queue message</returns>
    public SimpleStorageServiceQueueMessage<TEnvelope> ToSimpleStorageServiceQueueMessage(TEnvelope envelope,
        string payload) => new()
    {
        // Set the acknowledged timestamp into the response
        Acknowledged = Acknowledged,

        // Set the consumed timestamp into the response
        Consumed = Consumed,

        // Set the creation timestamp into the response
        Created = Created,

        // Set the decrypted envelope into the response
        Envelope = envelope,

        // Set the unique ID into the response
        Id = Id,

        // Set the decrypted payload into the response
        Payload = payload,

        // Set the published timestamp into the response
        Published = Published,

        // Set the rejection timestamp into the response
        Rejected = Rejected,

        // Set the rejection reason into the response
        RejectedReason = RejectedReason,

        // Set the S3 message into the response
        SimpleStorageServiceMessage = SimpleStorageServiceMessage
    };

    /// <summary>
    ///     This method converts the instance to a decrypted S3 queue message object
    /// </summary>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <returns>The decrypted S3 queue message object</returns>
    public SimpleStorageServiceQueueMessage<TEnvelope>
        ToSimpleStorageServiceQueueMessage(QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        ToSimpleStorageServiceQueueMessage(GetEnvelope(encryptionConfiguration),
            GetPayload(encryptionConfiguration) as string);

    /// <summary>
    ///     This method asynchronously converts the instance to a decrypted S3 queue message object
    /// </summary>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <returns>An awaitable task containing the decrypted S3 queue message object</returns>
    public async Task<SimpleStorageServiceQueueMessage<TEnvelope>>
        ToSimpleStorageServiceQueueMessageAsync(QueueServiceEncryptionConfiguration encryptionConfiguration)
    {
        // Localize the envelope
        TEnvelope envelope = await GetEnvelopeAsync(encryptionConfiguration);

        // Localize the payload
        string payload = await GetPayloadAsync(encryptionConfiguration) as string;

        // We're done, send the new message construct
        return ToSimpleStorageServiceQueueMessage(envelope, payload);
    }

    /// <summary>
    ///     This method fluidly resets the encrypted envelope into the instance
    /// </summary>
    /// <param name="envelope">The value to set into the instance</param>
    /// <returns>The current instance</returns>
    /// <exception cref="ArgumentException">When <paramref name="envelope" /> is NOT a valid cryptographic hash</exception>
    public SimpleStorageServiceEncryptedQueueMessage<TEnvelope> WithEnvelope(string envelope)
    {
        // Ensure we're working with an encrypted hash
        if (!CryptographyService.ValidateHash(envelope)) throw new ArgumentException("Invalid Envelope Hash");

        // Reset the envelope into the instance
        Envelope = envelope;

        // We're done, return the instance
        return this;
    }

    /// <summary>
    ///     This method fluidly encrypts <paramref name="envelope" /> and sets it into the instance
    /// </summary>
    /// <param name="envelope">The value to encrypt and set into the instance</param>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <returns>The current instance</returns>
    public SimpleStorageServiceEncryptedQueueMessage<TEnvelope> WithEnvelope(TEnvelope envelope,
        QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        WithEnvelope(Encrypt(envelope, encryptionConfiguration));

    /// <summary>
    ///     This method fluidly and asynchronously encrypts <paramref name="envelope" /> and sets it into the instance
    /// </summary>
    /// <param name="envelope">The value to encrypt and set into the instance</param>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <returns>An awaitable task containing the current instance</returns>
    public async Task<SimpleStorageServiceEncryptedQueueMessage<TEnvelope>> WithEnvelopeAsync(TEnvelope envelope,
        QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        WithEnvelope(await EncryptAsync(envelope, encryptionConfiguration));

    /// <summary>
    ///  This method fluidly resets the payload into the instance
    /// </summary>
    /// <param name="payload">The encrypted payload hash</param>
    /// <returns>The current instance</returns>
    /// <exception cref="ArgumentException">When <paramref name="payload" /> is NOT an encrypted hash</exception>
    public new SimpleStorageServiceEncryptedQueueMessage<TEnvelope> WithPayload(string payload)
    {
        // Ensure we have an already encrypted hash
        if (!CryptographyService.ValidateHash(payload)) throw new ArgumentException("Invalid Payload Hash");

        // Reset the payload into the instance
        Payload = payload;

        // We're done, return the instance
        return this;
    }

    /// <summary>
    ///     This method fluidly encrypts <paramref name="payload" /> and sets it into the instance
    /// </summary>
    /// <param name="payload">The value to encrypt and set into the instance</param>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <returns>The current instance</returns>
    public SimpleStorageServiceEncryptedQueueMessage<TEnvelope> WithPayload(string payload,
        QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        WithPayload(Encrypt(payload, encryptionConfiguration));

    /// <summary>
    /// This method fluidly and asynchronously encrypts <paramref name="payload" /> and sets it into the instance
    /// </summary>
    /// <param name="payload">The value to encrypt and set into the instance</param>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <returns>The current instance</returns>
    public async Task<SimpleStorageServiceEncryptedQueueMessage<TEnvelope>> WithPayloadAsync(string payload,
        QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        WithPayload(await EncryptAsync(payload, encryptionConfiguration));
}
