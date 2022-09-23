using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This class maintains the response error structure for a rejected queue message trace
/// </summary>
[XmlRoot("queueMessageRejectedReasonTrace")]
public class QueueMessageRejectedReasonTrace
{
    /// <summary>
    /// This property contains the class wherein the exception was triggered
    /// </summary>
    [JsonPropertyName("class")]
    [XmlAttribute("class")]
    public string Class { get; set; }

    /// <summary>
    /// This property contains the file in wherein the exception was triggered
    /// </summary>
    [JsonPropertyName("file")]
    [XmlAttribute("file")]
    public string File { get; set; }

    /// <summary>
    /// This property contains the line on which the exception was triggered
    /// </summary>
    [JsonPropertyName("line")]
    [XmlAttribute("line")]
    public int Line { get; set; }

    /// <summary>
    /// This property contains the namespace wherein the exception was triggered
    /// </summary>
    [JsonPropertyName("namespace")]
    [XmlAttribute("namespace")]
    public string Namespace { get; set; }

    /// <summary>
    /// This property contains the method wherein the exception was triggered
    /// </summary>
    [JsonPropertyName("method")]
    [XmlAttribute("method")]
    public string Method { get; set; }

    /// <summary>
    /// This property contains the source trace string
    /// </summary>
    [JsonPropertyName("source")]
    [XmlText]
    public string Source { get; set; }

    /// <summary>
    /// This method instantiates an empty response error trace entry
    /// </summary>
    public QueueMessageRejectedReasonTrace()
    {
    }

    /// <summary>
    /// This method instantiates a response error trace entry from an exception trace string
    /// </summary>
    /// <param name="traceString">The trace string from the system exception</param>
    public QueueMessageRejectedReasonTrace(string traceString)
    {
        // Instantiate our regular expression
        Regex pattern =
            new(@"^at\s+(?<method>(.*?))(\s+in\s+(?<file>(.*?)):line\s+(?<line>([0-9]+))?)", RegexOptions.Compiled |
                RegexOptions.IgnoreCase);

        // Match the pattern
        Match match = pattern.Match(traceString.Trim());

        // Make sure we have a valid trace string
        if (!string.IsNullOrEmpty(traceString) && match.Success)
        {
            // Set the source into the instance
            Source = traceString.Trim();

            // Split the methods into their parts
            List<string> methodParts = match.Groups["method"].Value.Split('.').ToList();

            // Set the class into the instance
            Class = methodParts.Skip(methodParts.Count - 2).FirstOrDefault();

            // Set the file into the instance
            File = match.Groups["file"].Value;

            // Set the line into the instance
            Line = !string.IsNullOrEmpty(match.Groups["line"].Value) &&
                   !string.IsNullOrWhiteSpace(match.Groups["line"].Value)
                ? Convert.ToInt32(match.Groups["line"].Value)
                : 0;

            // Set the method into the instance
            Method = methodParts.Skip(methodParts.Count - 1).FirstOrDefault();

            // Set the namespace into the instance
            Namespace = string.Join('.', methodParts.Take(methodParts.Count - 2));
        }
    }

    /// <summary>
    /// This method determines whether or not the trace entry is valid
    /// </summary>
    /// <returns></returns>
    public bool IsValid() => !string.IsNullOrEmpty(Method);
}
