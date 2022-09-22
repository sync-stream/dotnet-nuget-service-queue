using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Define our namespace
namespace SyncStream.Service.Queue.RabbitMq;

/// <summary>
///
/// </summary>
public class RabbitMqServiceSubscriber<TPayload> : BackgroundService
{
    /// <summary>
    /// This property contains the instance of our log service provider
    /// </summary>
    private readonly ILogger<RabbitMqServiceSubscriber<TPayload>> _logger;

    /// <summary>
    /// This property contains the instance of our queue service provider
    /// </summary>
    private readonly IRabbitMqService<TPayload> _service;

    /// <summary>
    /// This method instantiates the worker service with a <paramref name="logServiceProvider" />
    /// </summary>
    /// <param name="logServiceProvider">The log service provider</param>
    public RabbitMqServiceSubscriber(ILogger<RabbitMqServiceSubscriber<TPayload>> logServiceProvider)
    {
        // Set the log service provider into the instance
        _logger = logServiceProvider;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="logServiceProvider"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="defaultEndpoint"></param>
    public RabbitMqServiceSubscriber(ILogger<RabbitMqServiceSubscriber<TPayload>> logServiceProvider,
        IServiceProvider serviceProvider, RabbitMqQueueConfiguration defaultEndpoint = null)
    {
        // Set the log service provider into the instance
        _logger = logServiceProvider;

        // Set the queue service provider into the instance
        _service = new RabbitMqService<TPayload>(serviceProvider.GetService<ILogger<RabbitMqService<TPayload>>>(),
            defaultEndpoint);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="logServiceProvider"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="defaultEndpoint"></param>
    public RabbitMqServiceSubscriber(ILogger<RabbitMqServiceSubscriber<TPayload>> logServiceProvider,
        IServiceProvider serviceProvider, string defaultEndpoint = null)
    {
        // Set the log service provider into the instance
        _logger = logServiceProvider;

        // Set the queue service provider into the instance
        _service = new RabbitMqService<TPayload>(serviceProvider.GetService<ILogger<RabbitMqService<TPayload>>>(),
            defaultEndpoint);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken,
        ILogger<RabbitMqServiceSubscriber> logServiceProvider)
    {

    }
}
