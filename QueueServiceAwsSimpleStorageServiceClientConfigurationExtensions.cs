using SyncStream.Aws.S3.Client.Configuration;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This class maintains the structure of our S3ClientConfig extensions
/// </summary>
public static class QueueServiceAwsSimpleStorageServiceClientConfigurationExtensions
{
    /// <summary>
    /// This method provides a conversion from the queue's S3 configuration to the S3 client's
    /// </summary>
    /// <param name="instance">The current instance of S3ClientConfig</param>
    /// <param name="simpleStorageServiceConfiguration">The queue's S3 configuration</param>
    /// <returns>The current instance of S3ClientConfig with the properties updated</returns>
    public static AwsSimpleStorageServiceClientConfiguration FromQueueServiceConfiguration(
        this AwsSimpleStorageServiceClientConfiguration instance,
        QueueSimpleStorageServiceConfiguration simpleStorageServiceConfiguration)
    {
        // Set the access key ID into the S3 client configuration object instance
        instance.AccessKeyId = simpleStorageServiceConfiguration.AccessKeyId;

        // Set the KMS key ID into the S3 client configuration object instance
        instance.KeyManagementServiceKeyId = simpleStorageServiceConfiguration.KmsKeyId;

        // Set the region into the S3 client configuration object instance
        instance.Region = simpleStorageServiceConfiguration.Region;

        // Set the secret access key into the S3 client configuration object instance
        instance.SecretAccessKey = simpleStorageServiceConfiguration.SecretAccessKey;

        // We're done, return the S3 client configuration object instance
        return instance;
    }
}
