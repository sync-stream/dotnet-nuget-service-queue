using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This class maintains our IServiceCollection extensions
/// </summary>
public static class QueueServiceCollectionExtensions
{
    /// <summary>
    /// This method registers the global (default) queue encryption configuration
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="encryptionConfiguration">The queue encrypt configuration values</param>
    /// <returns><paramref name="instance" /></returns>
    public static IServiceCollection UseGlobalSyncStreamQueueEncryption(this IServiceCollection instance,
        QueueServiceEncryptionConfiguration encryptionConfiguration)
    {
        // Set the global (default) queue encryption configuration
        QueueService.RegisterDefaultEncryption(encryptionConfiguration);

        // We're done, return the instance
        return instance;
    }

    /// <summary>
    ///     This method registers the global (default) queue encryption configuration
    ///     from the <paramref name="section" /> of the application's <paramref name="configuration" />
    /// </summary>
    /// <param name="configuration">The application's configuration provider instance</param>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="section">
    ///     The section in the application's configuration that contains the queue encryption configuration
    /// </param>
    /// <returns><paramref name="instance" /></returns>
    public static IServiceCollection UseGlobalSyncStreamQueueEncryption(this IServiceCollection instance,
        IConfiguration configuration, string section) => UseGlobalSyncStreamQueueEncryption(instance,
        configuration.GetSection(section).Get<QueueServiceEncryptionConfiguration>());

    /// <summary>
    ///     This method registers the global (default) queue encryption configuration
    ///     from the <paramref name="section" /> of the application's configuration
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="section">
    ///     The section in the application's configuration that contains the queue encryption configuration
    /// </param>
    /// <returns><paramref name="instance" /></returns>
    public static IServiceCollection UseGlobalSyncStreamQueueEncryption(this IServiceCollection instance,
        IConfigurationSection section) =>
        UseGlobalSyncStreamQueueEncryption(instance, section.Get<QueueServiceEncryptionConfiguration>());

    /// <summary>
    ///     This method registers the global (default) queue encryption configuration from a provided
    ///     <paramref name="secret" /> and optional number of recursive <paramref name="passes" />
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="secret">The secret cryptographic key for encryption and decryption</param>
    /// <param name="passes">Optional, number of times to recursively encrypt the data</param>
    /// <returns><paramref name="instance" /></returns>
    public static IServiceCollection UseGlobalSyncStreamQueueEncryption(this IServiceCollection instance, string secret,
        int passes = 1) =>
        UseGlobalSyncStreamQueueEncryption(instance, new QueueServiceEncryptionConfiguration(secret, passes));

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
    ///     This method registers the global (default) queue to use with the QueueService from the
    ///     <paramref name="section"/> of the application's <paramref name="configuration" /> provider instance
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="configuration">The application's configuration provider</param>
    /// <param name="section">
    ///     The section in <paramref name="configuration" />
    ///     where the queue endpoint configuration is stored
    /// </param>
    /// <returns>The current IServiceCollection instance</returns>
    public static IServiceCollection UseGlobalSyncStreamQueueEndpoint(this IServiceCollection instance,
        IConfiguration configuration, string section) => UseGlobalSyncStreamQueueEndpoint(instance,
        configuration.GetSection(section).Get<QueueConfiguration>());

    /// <summary>
    ///     This method registers the global (default) queue to use with the QueueService from
    ///     the <paramref name="section" /> of the application's configuration provider instance
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="section">
    ///     The section in the application's configuration provider where the queue
    ///     endpoint configuration is stored
    /// </param>
    /// <returns>The current instance of IServiceCollection</returns>
    public static IServiceCollection UseGlobalSyncStreamQueueEndpoint(this IServiceCollection instance,
        IConfigurationSection section) => UseGlobalSyncStreamQueueEndpoint(instance, section.Get<QueueConfiguration>());

    /// <summary>
    /// This method registers the global (default) S3 configuration to use with the QueueService
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="simpleStorageServiceConfiguration">The AWS S3 configuration to use</param>
    /// <returns>The current IServiceCollection instance</returns>
    public static IServiceCollection UseGlobalSyncStreamQueueSimpleStorageService(this IServiceCollection instance,
        QueueSimpleStorageServiceConfiguration simpleStorageServiceConfiguration)
    {
        // Register the global/default S3 configuration
        QueueService.RegisterDefaultSimpleStorageServiceConfiguration(simpleStorageServiceConfiguration);

        // We're done, return the IServiceCollection instance
        return instance;
    }

    /// <summary>
    ///     This method registers the global (default) S3 configuration to use with the QueueService from the
    ///     <paramref name="section"/> of the application's <paramref name="configuration" /> provider instance
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="configuration">The application's configuration provider</param>
    /// <param name="section">
    ///     The section in <paramref name="configuration" />
    ///     where the AWS S3 configuration is stored
    /// </param>
    /// <returns>The current IServiceCollection instance</returns>
    public static IServiceCollection UseGlobalSyncStreamQueueSimpleStorageService(this IServiceCollection instance,
        IConfiguration configuration, string section) => UseGlobalSyncStreamQueueSimpleStorageService(instance,
        configuration.GetSection(section).Get<QueueSimpleStorageServiceConfiguration>());

    /// <summary>
    ///     This method registers the global (default) S3 configuration to use with the QueueService
    ///     from the <paramref name="section" /> of the application's configuration provider instance
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <param name="section">
    ///     The section in the application's configuration provider where the AWS S3 configuration is stored
    /// </param>
    /// <returns>The current instance of IServiceCollection</returns>
    public static IServiceCollection UseGlobalSyncStreamQueueSimpleStorageService(this IServiceCollection instance,
        IConfigurationSection section) =>
        UseGlobalSyncStreamQueueSimpleStorageService(instance, section.Get<QueueSimpleStorageServiceConfiguration>());

    /// <summary>
    /// This method registers a scoped publisher service with the <paramref name="instance" />
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <typeparam name="TPayload">The expected payload type to publish</typeparam>
    /// <returns>The current IServiceCollection instance</returns>
    public static IServiceCollection UseScopedSyncStreamQueuePublisher<TPayload>(this IServiceCollection instance)
    {
        // Register the scoped service
        instance.AddScoped<IQueueService<TPayload>, QueueService<TPayload>>();

        // We're done, return the IServiceCollection instance
        return instance;
    }

    /// <summary>
    /// This method registers a singleton publisher service with the <paramref name="instance" />
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <typeparam name="TPayload">The expected payload type to publish</typeparam>
    /// <returns>The current IServiceCollection instance</returns>
    public static IServiceCollection UseSingletonSyncStreamQueuePublisher<TPayload>(this IServiceCollection instance)
    {
        // Register the scoped service
        instance.AddSingleton<IQueueService<TPayload>, QueueService<TPayload>>();

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
    /// <typeparam name="TPayload">The expected message type from the queue</typeparam>
    /// <returns>The current instance of IServiceCollection</returns>
    public static IServiceCollection UseSyncStreamQueueSubscriber<TPayload>(this IServiceCollection instance,
        IQueueService.DelegateSubscriberAsync<TPayload> subscriber)
    {
        // Add our hosted service
        instance.AddHostedService(provider => new QueueSubscriberService<TPayload>(provider, subscriber));

        // We're done, return the IServiceCollection
        return instance;
    }

    /// <summary>
    /// This method registers an asynchronous subscriber for a RabbitMQ endpoint
    /// </summary>
    /// <param name="instance">The current instance of IServiceCollection</param>
    /// <param name="subscriber">The asynchronous subscriber delegate</param>
    /// <param name="defaultEndpoint">The queue endpoint definition</param>
    /// <param name="defaultSimpleStorageServiceConfiguration">Optional, S3 configuration</param>
    /// <param name="defaultEncryptionConfiguration">Optional, queue encryption configuration</param>
    /// <typeparam name="TPayload">The expected message type from the queue</typeparam>
    /// <returns>The current instance of IServiceCollection</returns>
    public static IServiceCollection UseSyncStreamQueueSubscriber<TPayload>(this IServiceCollection instance,
        IQueueService.DelegateSubscriberAsync<TPayload> subscriber, QueueConfiguration defaultEndpoint,
        QueueSimpleStorageServiceConfiguration defaultSimpleStorageServiceConfiguration = null,
        QueueServiceEncryptionConfiguration defaultEncryptionConfiguration = null)
    {
        // Add our hosted service
        instance.AddHostedService(provider => new QueueSubscriberService<TPayload>(provider, subscriber,
            defaultEndpoint, defaultSimpleStorageServiceConfiguration, defaultEncryptionConfiguration));

        // We're done, return the IServiceCollection
        return instance;
    }

    /// <summary>
    /// This method registers an asynchronous subscriber for a RabbitMQ endpoint
    /// </summary>
    /// <param name="instance">The current instance of IServiceCollection</param>
    /// <param name="subscriber">The asynchronous subscriber delegate</param>
    /// <param name="defaultEndpoint">The queue endpoint name</param>
    /// <param name="defaultSimpleStorageServiceConfiguration">Optional, S3 configuration</param>
    /// <param name="defaultEncryptionConfiguration">Optional, queue encryption configuration</param>
    /// <typeparam name="TPayload">The expected message type from the queue</typeparam>
    /// <returns>The current instance of IServiceCollection</returns>
    public static IServiceCollection UseSyncStreamQueueSubscriber<TPayload>(this IServiceCollection instance,
        IQueueService.DelegateSubscriberAsync<TPayload> subscriber, string defaultEndpoint,
        QueueSimpleStorageServiceConfiguration defaultSimpleStorageServiceConfiguration = null,
        QueueServiceEncryptionConfiguration defaultEncryptionConfiguration = null)
    {
        // Add our hosted service
        instance.AddHostedService(provider => new QueueSubscriberService<TPayload>(provider, subscriber,
            defaultEndpoint, defaultSimpleStorageServiceConfiguration, defaultEncryptionConfiguration));

        // We're done, return the IServiceCollection
        return instance;
    }

    /// <summary>
    /// This method registers a transient publisher service with the <paramref name="instance" />
    /// </summary>
    /// <param name="instance">The current IServiceCollection instance</param>
    /// <typeparam name="TPayload">The expected payload type to publish</typeparam>
    /// <returns>The current IServiceCollection instance</returns>
    public static IServiceCollection UseTransientSyncStreamQueuePublisher<TPayload>(this IServiceCollection instance)
    {
        // Register the scoped service
        instance.AddTransient<IQueueService<TPayload>, QueueService<TPayload>>();

        // We're done, return the IServiceCollection instance
        return instance;
    }
}
