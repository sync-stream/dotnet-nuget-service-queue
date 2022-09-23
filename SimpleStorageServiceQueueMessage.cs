using System.Text.Json.Serialization;
using System.Xml.Serialization;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This class maintains the structure of an S3 alias queue message
/// </summary>
/// <typeparam name="TPayload">The expected type of the envelope</typeparam>
[XmlInclude(typeof(QueueMessageRejectedReason))]
[XmlRoot("simpleStorageServiceQueueMessage")]
public class SimpleStorageServiceQueueMessage<TPayload> : QueueMessage<string>
{
    /// <summary>
    /// This property contains the timestamp at which the queue message was acknowledged
    /// </summary>
    [JsonPropertyName("acknowledged")]
    [XmlElement("acknowledged")]
    public DateTime? Acknowledged { get; set; }

    /// <summary>
    /// This property contains the original payload for the message
    /// </summary>
    [JsonPropertyName("envelope")]
    [XmlElement("envelope")]
    public TPayload Envelope { get; set; }

    /// <summary>
    /// This property contains the timestamp at which the message was rejected
    /// </summary>
    [JsonPropertyName("rejected")]
    [XmlElement("rejected")]
    public DateTime? Rejected { get; set; }

    /// <summary>
    /// This property contains the reason as to why the message was rejected
    /// </summary>
    [JsonPropertyName("rejectedReason")]
    [XmlElement("rejectedReason")]
    public QueueMessageRejectedReason RejectedReason { get; set; }

    /// <summary>
    /// This method generates an appropriately typed queue message from the instance
    /// </summary>
    /// <returns>A basic, typed QueueMessage</returns>
    public QueueMessage<TPayload> ToQueueMessage() => new()
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
        Published = Published
    };
}
