// Define our imports
using Microsoft.Extensions.Configuration;
using SyncStream.Service.Queue;

// Define our namespace
namespace SyncStreamQueueServiceExample;

/// <summary>
/// This interface maintains the configuration and startup of our application
/// </summary>
public interface ISyncStreamQueueServiceExampleServiceProvider
{
    /// <summary>
    /// This method does something
    /// </summary>
    /// <returns>An awaitable task with no result</returns>
    public Task DoSomething();
}

/// <summary>
/// This class maintains the configuration and startup of our application
/// </summary>
public class SyncStreamQueueServiceExampleServiceProvider : ISyncStreamQueueServiceExampleServiceProvider
{
    /// <summary>
    /// This property contains the application's configuration provider instance
    /// </summary>
    private readonly IConfiguration _configuration;

    /// <summary>
    /// This property contains the instance of our queue service provider
    /// </summary>
    private readonly IQueueService<ExampleQueueMessage> _queue;

    /// <summary>
    ///     This method instantiates our service provider with the application's
    ///     <paramref name="configuration" /> and a <paramref name="queueService" />
    /// </summary>
    /// <param name="configuration">The application's configuration</param>
    /// <param name="queueService">The queue service provider</param>
    public SyncStreamQueueServiceExampleServiceProvider(IConfiguration configuration,
        IQueueService<ExampleQueueMessage> queueService)
    {
        // Set the application configuration into the instance
        _configuration = configuration;

        // Set our queue service provider into the instance
        _queue = queueService;
    }

    /// <summary>
    /// This method does something
    /// </summary>
    /// <returns>An awaitable task with no result</returns>
    public async Task DoSomething()
    {
        // TODO - Something

        // Publish the message to the queue
        await _queue.PublishAsync(new("Something was done"));
    }
}
