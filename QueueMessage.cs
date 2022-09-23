﻿using System.Text.Json.Serialization;
using System.Xml.Serialization;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This class maintains the structure for a standardized Queue Message
/// </summary>
/// <typeparam name="TPayload">The message model type the queue message should deserialize to</typeparam>
[XmlInclude(typeof(QueueMessageRejectedReason))]
[XmlRoot("queueMessage")]
public class QueueMessage<TPayload>
{
    /// <summary>
    /// This property contains the timestamp at which the Queue Message was consumed
    /// </summary>
    [JsonPropertyName("consumed")]
    [XmlElement("consumed")]
    public DateTime? Consumed { get; set; }

    /// <summary>
    /// This property contains the timestamp at which the queue message was created
    /// </summary>
    [JsonPropertyName("created")]
    [XmlAttribute("created")]
    public DateTime Created { get; set; }

    /// <summary>
    /// This property contains the unique ID of the Queue Message
    /// </summary>
    [JsonPropertyName("id")]
    [XmlAttribute("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// This property contains the payload for the Queue Message
    /// </summary>
    [JsonPropertyName("payload")]
    [XmlElement("payload")]
    public TPayload Payload { get; set; }

    /// <summary>
    /// This property contains the timestamp at which the Queue Message was published to the queue
    /// </summary>
    [JsonPropertyName("published")]
    [XmlAttribute("published")]
    public DateTime? Published { get; set; }

    /// <summary>
    /// This method instantiates an empty Queue Message
    /// </summary>
    public QueueMessage()
    {
    }

    /// <summary>
    /// This method instantiates a new Queue Message with a <paramref name="payload" />
    /// </summary>
    /// <param name="payload">The message itself</param>
    public QueueMessage(TPayload payload) =>
        WithPayload(payload);

    /// <summary>
    /// This method resets the <paramref name="payload" /> into the instance
    /// </summary>
    /// <param name="payload">The message itself</param>
    /// <returns>This instance</returns>
    public QueueMessage<TPayload> WithPayload(TPayload payload)
    {
        // Reset the payload into the instance
        Payload = payload;

        // We're done, return the instance
        return this;
    }

    /// <summary>
    /// This method converts the instance to an S3 alias message
    /// </summary>
    /// <param name="objectName">The S3 object-key path to store the message as</param>
    /// <param name="acknowledged">Optional, the timestamp at which the message was acknowledged</param>
    /// <param name="rejected">Optional, the timestamp at which the message was rejected</param>
    /// <param name="reason">Optional, the reason the message was rejected</param>
    /// <returns>An instantiated S3 queue message alias</returns>
    public SimpleStorageServiceQueueMessage<TPayload> ToSimpleStorageServiceQueueMessage(string objectName,
        DateTime? acknowledged = null, DateTime? rejected = null, QueueMessageRejectedReason reason = null) => new()
    {
        // Set the acknowledged timestamp into the response
        Acknowledged = acknowledged,

        // Set the consumed timestamp into the response
        Consumed = Consumed,

        // Set the creation timestamp into the response
        Created = Created,

        // Set the unique message ID into the response
        Id = Id,

        // Set the payload envelope into the response
        Envelope = Payload,

        // Set the object name into the response
        Payload = objectName,

        // Set the published timestamp into the response
        Published = Published,

        // Set the rejection timestamp into the response
        Rejected = rejected,

        // Set the rejection reason into the response
        RejectedReason = reason
    };
}
