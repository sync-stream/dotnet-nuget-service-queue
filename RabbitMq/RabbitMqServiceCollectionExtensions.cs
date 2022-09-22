using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Define our namespace
namespace SyncStream.Service.Queue.RabbitMq;

/// <summary>
/// This class maintains our IServiceCollection extensions
/// </summary>
public static class RabbitMqServiceCollectionExtensions
{
    /// <summary>
    /// This method registers an endpoint configuration with the RabbitMQ service provider
    /// </summary>
    /// <param name="instance">The current instance of IServiceCollection</param>
    /// <param name="endpoint">The endpoint configuration to add</param>
    /// <returns>The current instance of IServiceCollection</returns>
    public static IServiceCollection UseSyncStreamRabbitMqEndpoint(this IServiceCollection instance,
        RabbitMqQueueConfiguration endpoint)
    {
        // Register the endpoint configuration
        RabbitMqService.RegisterEndpointConfiguration(endpoint);

        // We're done, return the IServiceCollection
        return instance;
    }

    /// <summary>
    /// This method registers multiple endpoint configurations with the RabbitMQ service provider
    /// </summary>
    /// <param name="instance">The current instance of IServiceCollection</param>
    /// <param name="endpoints">The endpoint configurations to add</param>
    /// <returns>The current instance of IServiceCollection</returns>
    public static IServiceCollection UseSyncStreamRabbitMqEndpoints(this IServiceCollection instance,
        IEnumerable<RabbitMqQueueConfiguration> endpoints)
    {
        // Register the endpoint configurations
        RabbitMqService.RegisterEndpointConfigurations(endpoints);

        // We're done, return the IServiceCollection
        return instance;
    }

    /// <summary>
    /// This method registers multiple endpoint configurations with the RabbitMQ service provider
    /// </summary>
    /// <param name="instance">The current instance of IServiceCollection</param>
    /// <param name="endpoints">The endpoint configurations to add</param>
    /// <returns>The current instance of IServiceCollection</returns>
    public static IServiceCollection UseSyncStreamRabbitMqEndpoints(this IServiceCollection instance,
        params RabbitMqQueueConfiguration[] endpoints)
    {
        // Register the endpoint configurations
        RabbitMqService.RegisterEndpointConfigurations(endpoints);

        // We're done, return the IServiceCollection
        return instance;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="subscriber"></param>
    /// <param name="defaultEndpoint"></param>
    /// <typeparam name="TPayload"></typeparam>
    /// <returns></returns>
    public static IServiceCollection UseSyncStreamRabbitMqSubscriber<TPayload>(this IServiceCollection instance,
        IRabbitMqService<TPayload>.DelegateSubscriber subscriber, RabbitMqQueueConfiguration defaultEndpoint = null)
    {

        // Add our hosted service
        instance.AddHostedService(provider =>
            new RabbitMqServiceSubscriber<TPayload>(provider.GetService<ILogger<RabbitMqServiceSubscriber<TPayload>>>(),
                provider, defaultEndpoint));

        // We're done, return the IServiceCollection
        return instance;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="subscriber"></param>
    /// <param name="defaultEndpoint"></param>
    /// <typeparam name="TPayload"></typeparam>
    /// <returns></returns>
    public static IServiceCollection UseSyncStreamRabbitMqSubscriber<TPayload>(this IServiceCollection instance,
        IRabbitMqService<TPayload>.DelegateSubscriber subscriber, string defaultEndpoint = null)
    {

        // Add our hosted service
        instance.AddHostedService(provider =>
            new RabbitMqServiceSubscriber<TPayload>(provider.GetService<ILogger<RabbitMqServiceSubscriber<TPayload>>>(),
                provider, defaultEndpoint));

        // We're done, return the IServiceCollection
        return instance;
    }
}
