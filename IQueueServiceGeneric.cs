// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This interface maintains the structure of our queue service provider
/// </summary>
/// <typeparam name="TModel">The typed-result expected from the queue</typeparam>
public interface IQueueService<TModel>
{
    /// <summary>
    /// This delegate defines the work-item structure
    /// </summary>
    delegate ValueTask<TModel> WorkItemDelegate(CancellationToken stoppingToken);

    /// <summary>
    /// This method asynchronously publishes an <paramref name="item" /> to the queue
    /// </summary>
    /// <param name="item">The item to publish</param>
    /// <returns>An awaitable value task</returns>
    ValueTask PublishAsync(WorkItemDelegate item);

    /// <summary>
    /// This method asynchronously consumes an item from the queue
    /// </summary>
    /// <param name="cancellationToken">The token denoting cancellation</param>
    /// <returns>An awaitable task containing an invocable item from the queue</returns>
    ValueTask<WorkItemDelegate> ConsumeAsync(CancellationToken cancellationToken);
}
