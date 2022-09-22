using System.Threading.Channels;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This class maintains the structure of our queue service provider
/// </summary>
/// <typeparam name="TModel">The typed-result expected from the queue</typeparam>
public class QueueService<TModel> : IQueueService<TModel>
{
    /// <summary>
    /// This property contains our internal queue
    /// </summary>
    private readonly Channel<IQueueService<TModel>.WorkItemDelegate> _queue;

    /// <summary>
    /// This method instantiates our queue service provider with an optional <paramref name="capacity" />
    /// </summary>
    /// <param name="capacity">Capacity of the queue</param>
    public QueueService(int capacity = 1024)
    {
        // Define our channel options
        BoundedChannelOptions options = new(capacity) {FullMode = BoundedChannelFullMode.Wait};

        // Create our bound queue channel
        _queue = Channel.CreateBounded<IQueueService<TModel>.WorkItemDelegate>(options);
    }

    /// <summary>
    /// This method asynchronously publishes an <paramref name="item" /> to the queue
    /// </summary>
    /// <param name="item">The item to publish</param>
    /// <returns>An awaitable value task</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="item" /> is null</exception>
    public ValueTask PublishAsync(IQueueService<TModel>.WorkItemDelegate item)
    {
        // Ensure we have an item
        if (item is null) throw new ArgumentNullException(nameof(item));

        // We're done, publish the item
        return _queue.Writer.WriteAsync(item);
    }

    /// <summary>
    /// This method asynchronously consumes an item from the queue
    /// </summary>
    /// <param name="cancellationToken">The token denoting cancellation</param>
    /// <returns>An awaitable task containing an invocable item from the queue</returns>
    public ValueTask<IQueueService<TModel>.WorkItemDelegate> ConsumeAsync(CancellationToken cancellationToken) =>
        _queue.Reader.ReadAsync(cancellationToken);
}
