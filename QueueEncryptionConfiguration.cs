using System.Xml.Serialization;
using SyncStream.Cryptography.Configuration;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This class maintains the structure of a queue's encryption configuration
/// </summary>
[XmlRoot("QueueEncryptionConfiguration")]
public class QueueServiceEncryptionConfiguration : CryptographyConfiguration
{
    /// <summary>
    /// This method instantiates an empty configuration
    /// </summary>
    public QueueServiceEncryptionConfiguration()
    {
    }

    /// <summary>
    ///     This method constructs a configuration object with a <paramref name="secret" />
    ///     and optional number or <paramref name="passes" />
    /// </summary>
    /// <param name="secret">The secret key for encryption and decryption</param>
    /// <param name="passes">Optional, number of times to recursively encrypt the data</param>
    public QueueServiceEncryptionConfiguration(string secret, int passes = 1)
    {
        // Set the passes into the instance
        Passes = passes;

        // Set the secret into the instance
        Secret = secret;
    }
}
