using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This class maintains the structure of our RabbitMQ subscribers background service
/// </summary>
public class QueueSubscriberService<TPayload> : BackgroundService
{
    /// <summary>
    /// This property contains the instance of our log service provider
    /// </summary>
    private readonly ILogger<QueueSubscriberService<TPayload>> _logger;

    /// <summary>
    /// This property contains the instance of our queue service provider
    /// </summary>
    private readonly IQueueService<TPayload> _service;

    /// <summary>
    /// This property contains our asynchronous worker delegate
    /// </summary>
    private readonly IQueueService.DelegateSubscriberAsync<TPayload> _subscriber;

    /// <summary>
    /// This method instantiates an asynchronous subscriber worker from an external service provider
    /// </summary>
    /// <param name="logServiceProvider">The log service provider to use in the worker</param>
    /// <param name="serviceProvider">The external service provider</param>
    /// <param name="subscriber">The asynchronous subscriber delegate</param>
    public QueueSubscriberService(ILogger<QueueSubscriberService<TPayload>> logServiceProvider,
        IServiceProvider serviceProvider, IQueueService.DelegateSubscriberAsync<TPayload> subscriber)
    {
        // Set the log service provider into the instance
        _logger = logServiceProvider;

        // Set the queue service provider into the instance
        _service = new QueueService<TPayload>(serviceProvider.GetService<ILogger<QueueService<TPayload>>>());

        // Set the subscriber into the instance
        _subscriber = subscriber;
    }

    /// <summary>
    /// This method instantiates an asynchronous subscriber worker from an external service provider
    /// </summary>
    /// <param name="logServiceProvider">The log service provider to use in the worker</param>
    /// <param name="serviceProvider">The external service provider</param>
    /// <param name="subscriber">The asynchronous subscriber delegate</param>
    /// <param name="defaultEndpoint">Optional, RabbitMQ endpoint definition</param>
    /// <param name="defaultSimpleStorageServiceConfiguration">Optional, S3 configuration to use</param>
    public QueueSubscriberService(ILogger<QueueSubscriberService<TPayload>> logServiceProvider,
        IServiceProvider serviceProvider, IQueueService.DelegateSubscriberAsync<TPayload> subscriber,
        QueueConfiguration defaultEndpoint = null,
        QueueSimpleStorageServiceConfiguration defaultSimpleStorageServiceConfiguration = null)
    {
        // Set the log service provider into the instance
        _logger = logServiceProvider;

        // Set the queue service provider into the instance
        _service = new QueueService<TPayload>(serviceProvider.GetService<ILogger<QueueService<TPayload>>>(),
            defaultEndpoint, defaultSimpleStorageServiceConfiguration);

        // Set the subscriber into the instance
        _subscriber = subscriber;
    }

    /// <summary>
    /// This method instantiates an asynchronous subscriber worker from an external service provider
    /// </summary>
    /// <param name="logServiceProvider">The log service provider to use in the worker</param>
    /// <param name="serviceProvider">The external service provider</param>
    /// <param name="subscriber">The asynchronous subscriber delegate</param>
    /// <param name="defaultEndpoint">Optional, RabbitMQ endpoint name</param>
    /// <param name="defaultSimpleStorageServiceConfiguration">Optional, S3 configuration</param>
    public QueueSubscriberService(ILogger<QueueSubscriberService<TPayload>> logServiceProvider,
        IServiceProvider serviceProvider, IQueueService.DelegateSubscriberAsync<TPayload> subscriber,
        string defaultEndpoint = null,
        QueueSimpleStorageServiceConfiguration defaultSimpleStorageServiceConfiguration = null)
    {
        // Set the log service provider into the instance
        _logger = logServiceProvider;

        // Set the queue service provider into the instance
        _service = new QueueService<TPayload>(serviceProvider.GetService<ILogger<QueueService<TPayload>>>(),
            defaultEndpoint, defaultSimpleStorageServiceConfiguration);

        // Set the subscriber into the instance
        _subscriber = subscriber;
    }

    /// <summary>
    /// This method asynchronously runs the subscriber
    /// </summary>
    /// <param name="stoppingToken">The token denoting cancellation</param>
    /// <returns>An awaitable task with a void result</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Iterate until cancellation has been requested
        while (!stoppingToken.IsCancellationRequested) await _service.SubscribeAsync(_subscriber, stoppingToken);
    }
}
