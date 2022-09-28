namespace SyncStream.Service.Queue;

/// <summary>
///     This interface maintains the structure of our S3 queue message
/// </summary>
/// <typeparam name="TPayload">The expected type of the envelope</typeparam>
public interface ISimpleStorageServiceQueueMessage<TPayload>
{
    /// <summary>
    ///     This property contains the timestamp at which the queue message was acknowledged
    /// </summary>
    public DateTime? Acknowledged { get; set; }

    /// <summary>
    ///     This property contains the original payload for the message
    /// </summary>
    public TPayload Envelope { get; set; }

    /// <summary>
    ///     This property contains the timestamp at which the message was rejected
    /// </summary>
    public DateTime? Rejected { get; set; }

    /// <summary>
    ///     This property contains the reason as to why the message was rejected
    /// </summary>
    public QueueMessageRejectedReason RejectedReason { get; set; }
}
