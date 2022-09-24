// Define our imports
using System.Text.Json.Serialization;
using System.Xml.Serialization;

// Define our namespace
namespace SyncStreamQueueServiceExample;

/// <summary>
///  This class maintains the structure of our example queue message
/// </summary>
[XmlRoot("QueueMessage")]
public class ExampleQueueMessage
{
    /// <summary>
    /// This enum maintains our sources for the queue messages
    /// </summary>
    public enum MessageSource
    {
        /// <summary>
        /// This enum value defines the api source
        /// </summary>
        Api,

        /// <summary>
        /// This enum value defines the service-worker source
        /// </summary>
        ServiceWorker,

        /// <summary>
        /// This enum value defines the web-application source
        /// </summary>
        WebApp
    }

    /// <summary>
    /// This property contains the unique publisher ID of the message
    /// </summary>
    [JsonPropertyName("id")]
    [XmlAttribute("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// This property contains the source of the queue message
    /// </summary>
    [JsonPropertyName("source")]
    [XmlAttribute("source")]
    public MessageSource Source { get; set; } = MessageSource.ServiceWorker;

    /// <summary>
    /// This property contains the text to send to the subscriber
    /// </summary>
    [JsonPropertyName("text")]
    [XmlText]
    public string Text { get; set; }

    /// <summary>
    /// This method instantiates a new and empty queue message
    /// </summary>
    public ExampleQueueMessage()
    {
    }

    /// <summary>
    /// This method instantiates a new queue message with the <paramref name="text" /> to send
    /// </summary>
    /// <param name="text">The text to send</param>
    public ExampleQueueMessage(string text) => Text = text;

    /// <summary>
    /// This method instantiates a new queue message with a <paramref name="source" /> and optional <paramref name="text" />
    /// </summary>
    /// <param name="source">The MessageSource of the message</param>
    /// <param name="text">Optional, text to send</param>
    public ExampleQueueMessage(MessageSource source = MessageSource.ServiceWorker, string text = null)
    {
        // Set the source of the queue message into the instance
        Source = source;

        // Set the text to send into the instance
        Text = text;
    }
}
