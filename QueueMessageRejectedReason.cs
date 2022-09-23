using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This class maintains the structure of our queue message rejected reason
/// </summary>
[XmlInclude(typeof(QueueMessageRejectedReasonTrace))]
[XmlInclude(typeof(List<QueueMessageRejectedReasonTrace>))]
[XmlRoot("queueMessageRejectedReason")]
public class QueueMessageRejectedReason
{
    /// <summary>
    /// This method provides implicit conversion between strings and rejection reasons
    /// </summary>
    /// <param name="message">The message for the rejection reason</param>
    /// <returns>An instantiated queue message rejection reason</returns>
    public static implicit operator QueueMessageRejectedReason(string message) => new(message);

    /// <summary>
    /// This method provides implicit conversion between exceptions and rejection reasons
    /// </summary>
    /// <param name="exception">The exception for the rejection reason</param>
    /// <returns>An instantiated queue message rejection reason</returns>
    public static implicit operator QueueMessageRejectedReason(Exception exception) => new(exception);

    /// <summary>
    /// This property contains an inner error if one occurred
    /// </summary>
    [JsonPropertyName("innerRejection")]
    [XmlElement("innerRejection")]
    public QueueMessageRejectedReason InnerError { get; set; }

    /// <summary>
    /// This property contains the message associated with the error
    /// </summary>
    [JsonPropertyName("message")]
    [XmlElement("message")]
    public string Message { get; set; }

    /// <summary>
    /// This property contains the trace from the exception if it exists
    /// </summary>
    [JsonPropertyName("trace")]
    [XmlElement("trace")]
    public List<QueueMessageRejectedReasonTrace> Trace { get; set; }

    /// <summary>
    /// This property contains the type of message that occurred
    /// </summary>
    [JsonPropertyName("type")]
    [XmlAttribute("type")]
    public string Type { get; set; }

    /// <summary>
    /// This property denotes whether the API request was successful or not
    /// </summary>
    [JsonPropertyName("success")]
    [XmlAttribute("successful")]
    public bool Successful { get; set; }

    /// <summary>
    /// This method converts a standard exception to a specific error
    /// </summary>
    /// <param name="exception"></param>
    /// <typeparam name="TError"></typeparam>
    /// <returns></returns>
    public static TError FromException<TError>(Exception exception) where TError : QueueMessageRejectedReason, new() =>
        (TError) Activator.CreateInstance(typeof(TError), exception);

    /// <summary>
    /// This method instantiates an empty response error
    /// </summary>
    /// <param name="message">The rejection reason</param>
    public QueueMessageRejectedReason(string message)
    {
        // Set the message into the instance
        Message = message;
    }

    /// <summary>
    /// This method instantiates a populates response error
    /// </summary>
    /// <param name="exception"></param>
    protected QueueMessageRejectedReason(Exception exception)
    {
        // Check for an inner-exception and set it into the instance
        InnerError = exception.InnerException == null ? null : new(exception.InnerException);

        // Set the message into the instance
        Message = exception.Message;

        // Set the trace into the instance
        Trace = new();

        // Set the success flag into the instance
        Successful = false;

        // Set the exception type into the instance
        Type = exception.GetType().ToString();

        // Check for a stack trace and process it into the instance
        if (!string.IsNullOrEmpty(exception.StackTrace) && !string.IsNullOrWhiteSpace(exception.StackTrace))
            exception.StackTrace.Split("\n").Select(t => t.Trim()).ToList().ForEach(t =>
            {
                // Localize our trace entry
                QueueMessageRejectedReasonTrace entry = new(t);

                // Add the entry to the trace if it's valid
                if (entry.IsValid()) Trace.Add(new(t));
            });
    }

    /// <summary>
    /// This method clears the trace from the error
    /// </summary>
    public void ClearTrace()
    {
        // Clear the main trace
        Trace?.Clear();

        // Clear any nested traces
        InnerError?.ClearTrace();
    }
}
