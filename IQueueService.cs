// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This interface maintains the structure of our queue service provider
/// </summary>
public interface IQueueService
{
    /// <summary>
    /// This delegate defines the work-item structure
    /// </summary>
    delegate ValueTask<object> WorkItemDelegate(CancellationToken stoppingToken);

    /// <summary>
    /// This method asynchronously publishes an <paramref name="item" /> to the queue
    /// </summary>
    /// <param name="item">The item to publish</param>
    /// <returns>An awaitable value task</returns>
    ValueTask PublishAsync(WorkItemDelegate item);
}
