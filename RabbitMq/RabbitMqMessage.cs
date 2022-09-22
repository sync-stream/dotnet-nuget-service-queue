using System.Text.Json.Serialization;
using System.Xml.Serialization;

// Define our namespace
namespace SyncStream.Service.Queue.RabbitMq;

/// <summary>
/// This class maintains the structure for a standardized Queue Message
/// </summary>
/// <typeparam name="TPayload">The message model type the queue message should deserialize to</typeparam>
[XmlRoot("queueMessage")]
public class RabbitMqMessage<TPayload>
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
    [XmlElement("created")]
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
    public RabbitMqMessage()
    {
    }

    /// <summary>
    /// This method instantiates a new Queue Message with a <paramref name="payload" />
    /// </summary>
    /// <param name="payload">The message itself</param>
    public RabbitMqMessage(TPayload payload) =>
        WithPayload(payload);

    /// <summary>
    /// This method resets the <paramref name="payload" /> into the instance
    /// </summary>
    /// <param name="payload">The message itself</param>
    /// <returns>This instance</returns>
    public RabbitMqMessage<TPayload> WithPayload(TPayload payload)
    {
        // Reset the payload into the instance
        Payload = payload;

        // We're done, return the instance
        return this;
    }
}
