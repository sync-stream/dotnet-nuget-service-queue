using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This class maintains our IServiceCollection extensions
/// </summary>
public static class QueueServiceCollectionExtensions
{
    /// <summary>
    /// This method registers an endpoint configuration with the RabbitMQ service provider
    /// </summary>
    /// <param name="instance">The current instance of IServiceCollection</param>
    /// <param name="endpoint">The endpoint configuration to add</param>
    /// <returns>The current instance of IServiceCollection</returns>
    public static IServiceCollection UseSyncStreamQueueEndpoint(this IServiceCollection instance,
        QueueConfiguration endpoint)
    {
        // Register the endpoint configuration
        QueueService.RegisterEndpointConfiguration(endpoint);

        // We're done, return the IServiceCollection
        return instance;
    }

    /// <summary>
    /// This method registers multiple endpoint configurations with the RabbitMQ service provider
    /// </summary>
    /// <param name="instance">The current instance of IServiceCollection</param>
    /// <param name="endpoints">The endpoint configurations to add</param>
    /// <returns>The current instance of IServiceCollection</returns>
    public static IServiceCollection UseSyncStreamQueueEndpoints(this IServiceCollection instance,
        IEnumerable<QueueConfiguration> endpoints)
    {
        // Register the endpoint configurations
        QueueService.RegisterEndpointConfigurations(endpoints);

        // We're done, return the IServiceCollection
        return instance;
    }

    /// <summary>
    /// This method registers multiple endpoint configurations with the RabbitMQ service provider
    /// </summary>
    /// <param name="instance">The current instance of IServiceCollection</param>
    /// <param name="endpoints">The endpoint configurations to add</param>
    /// <returns>The current instance of IServiceCollection</returns>
    public static IServiceCollection UseSyncStreamQueueEndpoints(this IServiceCollection instance,
        params QueueConfiguration[] endpoints)
    {
        // Register the endpoint configurations
        QueueService.RegisterEndpointConfigurations(endpoints);

        // We're done, return the IServiceCollection
        return instance;
    }

    /// <summary>
    /// This method registers an asynchronous subscriber for a RabbitMQ endpoint
    /// </summary>
    /// <param name="instance">The current instance of IServiceCollection</param>
    /// <param name="subscriber">The asynchronous subscriber delegate</param>
    /// <param name="defaultEndpoint">Optional endpoint definition</param>
    /// <typeparam name="TPayload">The expected message type from the queue</typeparam>
    /// <returns>The current instance of IServiceCollection</returns>
    public static IServiceCollection UseSyncStreamSyncStreamSubscriber<TPayload>(this IServiceCollection instance,
        IQueueService.DelegateSubscriberAsync<TPayload> subscriber,
        QueueConfiguration defaultEndpoint = null)
    {
        // Add our hosted service
        instance.AddHostedService(provider =>
            new QueueSubscriberService<TPayload>(provider.GetService<ILogger<QueueSubscriberService<TPayload>>>(),
                provider, subscriber, defaultEndpoint));

        // We're done, return the IServiceCollection
        return instance;
    }

    /// <summary>
    /// This method registers an asynchronous subscriber for a RabbitMQ endpoint
    /// </summary>
    /// <param name="instance">The current instance of IServiceCollection</param>
    /// <param name="subscriber">The asynchronous subscriber delegate</param>
    /// <param name="defaultEndpoint">Optional endpoint name</param>
    /// <typeparam name="TPayload">The expected message type from the queue</typeparam>
    /// <returns>The current instance of IServiceCollection</returns>
    public static IServiceCollection UseSyncStreamQueueSubscriber<TPayload>(this IServiceCollection instance,
        IQueueService.DelegateSubscriberAsync<TPayload> subscriber, string defaultEndpoint = null)
    {
        // Add our hosted service
        instance.AddHostedService(provider =>
            new QueueSubscriberService<TPayload>(provider.GetService<ILogger<QueueSubscriberService<TPayload>>>(),
                provider, subscriber, defaultEndpoint));

        // We're done, return the IServiceCollection
        return instance;
    }
}
