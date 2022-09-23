using System.Text.Json.Serialization;
using System.Xml.Serialization;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This class maintains the structure of an S3 backed queue
/// </summary>
[XmlRoot("QueueSimpleStorageServiceConfiguration")]
public class QueueSimpleStorageServiceConfiguration
{
    /// <summary>
    /// This property contains the AWS S3 authentication access_key_id
    /// </summary>
    [JsonPropertyName("AccessKeyId")]
    [XmlAttribute("accessKeyId")]
    public string AccessKeyId { get; set; }

    /// <summary>
    /// This property contains the S3 bucket to store the messages in
    /// </summary>
    [JsonPropertyName("Bucket")]
    [XmlAttribute("bucket")]
    public string Bucket { get; set; }

    /// <summary>
    /// This property contains the unique ID of the AWS Key Management Service Key used to encrypt objects in the bucket
    /// </summary>
    [JsonPropertyName("KmsKeyId")]
    [XmlAttribute("kmsKeyId")]
    public string KmsKeyId { get; set; }

    /// <summary>
    /// This property contains the AWS region
    /// </summary>
    [JsonPropertyName("Region")]
    [XmlAttribute("region")]
    public string Region { get; set; } = "us-east-1";

    /// <summary>
    /// This property contains the AWS S3 authentication secret_access_key
    /// </summary>
    [JsonPropertyName("SecretAccessKey")]
    [XmlAttribute("secretAccessKey")]
    public string SecretAccessKey { get; set; }
}
