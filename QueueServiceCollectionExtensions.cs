using Microsoft.Extensions.Configuration;
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
    /// This method registers the global (default) queue to use with the QueueService
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="endpoint">The endpoint to use globally and by default</param>
    /// <returns><paramref name="instance" /></returns>
    public static IServiceCollection UseGlobalSyncStreamQueueEndpoint(this IServiceCollection instance,
        QueueConfiguration endpoint)
    {
        // Register the global/default endpoint
        QueueService.RegisterDefaultEndpoint(endpoint);

        // We're done, return the IServiceCollection instance
        return instance;
    }

    /// <summary>
    /// This method registers the global (default) S3 configuration to use with the QueueService
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="simpleStorageServiceConfiguration"></param>
    /// <returns></returns>
    public static IServiceCollection UseGlobalSyncStreamQueueSimpleStorageService(this IServiceCollection instance,
        QueueSimpleStorageServiceConfiguration simpleStorageServiceConfiguration)
    {
        // Register the global/default S3 configuration
        QueueService.RegisterDefaultSimpleStorageServiceConfiguration(simpleStorageServiceConfiguration);

        // We're done, return the IServiceCollection instance
        return instance;
    }

    /// <summary>
    /// This method registers an endpoint configuration with the RabbitMQ service provider
    /// </summary>
    /// <param name="instance">The current instance of IServiceCollection</param>
    /// <param name="endpoint">The endpoint configuration to add</param>
    /// <param name="defaultQueue">Optionally set this as the default queue</param>
    /// <returns>The current instance of IServiceCollection</returns>
    public static IServiceCollection UseSyncStreamQueueEndpoint(this IServiceCollection instance,
        QueueConfiguration endpoint, bool defaultQueue = false)
    {
        // Register the endpoint configuration
        QueueService.RegisterEndpointConfiguration(endpoint);

        // We're done, return the IServiceCollection instance
        return instance;
    }

    /// <summary>
    /// This method registers an endpoint configurations with the RabbitMQ service provider from an existing configuration
    /// </summary>
    /// <param name="instance">The current instance of IServiceCollection</param>
    /// <param name="configuration">The existing IConfiguration instance</param>
    /// <param name="section">The section where the queue configurations live in the <paramref name="configuration" /></param>
    /// <returns>The current IServiceCollection instance</returns>
    public static IServiceCollection UseSyncStreamQueueEndpoint(this IServiceCollection instance,
        IConfiguration configuration, string section) => UseSyncStreamQueueEndpoint(instance,
        configuration.GetSection(section).Get<QueueConfiguration>());

    /// <summary>
    /// This method registers an endpoint configurations with the RabbitMQ service provider from an existing configuration section
    /// </summary>
    /// <param name="instance">The current instance of IServiceCollection</param>
    /// <param name="section">The existing IConfigurationSection instance</param>
    /// <returns>The current IServiceCollection instance</returns>
    public static IServiceCollection UseSyncStreamQueueEndpoint(this IServiceCollection instance,
        IConfigurationSection section) => UseSyncStreamQueueEndpoint(instance, section.Get<QueueConfiguration>());

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

        // We're done, return the IServiceCollection instance
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

        // We're done, return the IServiceCollection instance
        return instance;
    }

    /// <summary>
    /// This method registers multiple endpoint configurations with the RabbitMQ service provider from an existing configuration
    /// </summary>
    /// <param name="instance">The current instance of IServiceCollection</param>
    /// <param name="configuration">The existing IConfiguration instance</param>
    /// <param name="section">The section where the queue configurations live in the <paramref name="configuration" /></param>
    /// <returns>The current IServiceCollection instance</returns>
    public static IServiceCollection UseSyncStreamQueueEndpoints(this IServiceCollection instance,
        IConfiguration configuration, string section) => UseSyncStreamQueueEndpoints(instance,
        configuration.GetSection(section).Get<List<QueueConfiguration>>());

    /// <summary>
    /// This method registers multiple endpoint configurations with the RabbitMQ service provider from an existing configuration section
    /// </summary>
    /// <param name="instance">The current instance of IServiceCollection</param>
    /// <param name="section">The existing IConfigurationSection instance</param>
    /// <returns>The current IServiceCollection instance</returns>
    public static IServiceCollection UseSyncStreamQueueEndpoints(this IServiceCollection instance,
        IConfigurationSection section) =>
        UseSyncStreamQueueEndpoints(instance, section.Get<List<QueueConfiguration>>());

    /// <summary>
    /// This method registers an asynchronous subscriber for a RabbitMQ endpoint
    /// </summary>
    /// <param name="instance">The current instance of IServiceCollection</param>
    /// <param name="subscriber">The asynchronous subscriber delegate</param>
    /// <param name="defaultEndpoint">Optional, endpoint definition</param>
    /// <param name="defaultSimpleStorageServiceConfiguration">Optional, S3 configuration</param>
    /// <typeparam name="TPayload">The expected message type from the queue</typeparam>
    /// <returns>The current instance of IServiceCollection</returns>
    public static IServiceCollection UseSyncStreamSyncStreamSubscriber<TPayload>(this IServiceCollection instance,
        IQueueService.DelegateSubscriberAsync<TPayload> subscriber,
        QueueConfiguration defaultEndpoint = null,
        QueueSimpleStorageServiceConfiguration defaultSimpleStorageServiceConfiguration = null)
    {
        // Add our hosted service
        instance.AddHostedService(provider =>
            new QueueSubscriberService<TPayload>(provider.GetService<ILogger<QueueSubscriberService<TPayload>>>(),
                provider, subscriber, defaultEndpoint, defaultSimpleStorageServiceConfiguration));

        // We're done, return the IServiceCollection
        return instance;
    }

    /// <summary>
    /// This method registers an asynchronous subscriber for a RabbitMQ endpoint
    /// </summary>
    /// <param name="instance">The current instance of IServiceCollection</param>
    /// <param name="subscriber">The asynchronous subscriber delegate</param>
    /// <param name="defaultEndpoint">Optional, endpoint name</param>
    /// <param name="defaultSimpleStorageServiceConfiguration">Optional, S3 configuration</param>
    /// <typeparam name="TPayload">The expected message type from the queue</typeparam>
    /// <returns>The current instance of IServiceCollection</returns>
    public static IServiceCollection UseSyncStreamQueueSubscriber<TPayload>(this IServiceCollection instance,
        IQueueService.DelegateSubscriberAsync<TPayload> subscriber, string defaultEndpoint = null,
        QueueSimpleStorageServiceConfiguration defaultSimpleStorageServiceConfiguration = null)
    {
        // Add our hosted service
        instance.AddHostedService(provider =>
            new QueueSubscriberService<TPayload>(provider.GetService<ILogger<QueueSubscriberService<TPayload>>>(),
                provider, subscriber, defaultEndpoint, defaultSimpleStorageServiceConfiguration));

        // We're done, return the IServiceCollection
        return instance;
    }
}
