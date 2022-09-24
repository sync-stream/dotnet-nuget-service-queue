// Define our imports
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SyncStream.Service.Queue;

// Define our namespace
namespace SyncStreamQueueServiceExample;

/// <summary>
/// This class maintains the configuration and startup of our application
/// </summary>
public class Startup
{
    /// <summary>
    /// This property contains the application's configuration provider instance
    /// </summary>
    public readonly IConfiguration Configuration;

    /// <summary>
    /// This method instantiates our startup with the application's <paramref name="configuration" /> provider instance
    /// </summary>
    /// <param name="configuration">The application's configuration provider instance</param>
    public Startup(IConfiguration configuration) => Configuration = configuration;

    /// <summary>
    /// This method is responsible for processing messages from the queue
    /// </summary>
    /// <param name="message">The message brokered from the queue</param>
    /// <param name="stoppingToken">The token responsible for task cancellation</param>
    /// <returns>An awaitable task with a void result</returns>
    private Task HandleQueueMessage(QueueMessage<ExampleQueueMessage> message,
        CancellationToken stoppingToken = default)
    {
        // Ensure the token hasn't been cancelled
        if (stoppingToken.IsCancellationRequested) Console.WriteLine("Subscriber Cancelled");

        // We're good, write the message to console
        else Console.WriteLine($"Queue Message Received:\t{message.Payload}");

        // Log the original message
        Console.WriteLine($@"\n\n\n{JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            // Don't ignore any nulls on serialization
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,

            // We want pretty JSON
            WriteIndented = true
        })}\n\n\n");

        // We're done, return the completed task
        return Task.CompletedTask;
    }

    /// <summary>
    /// This method is called by the framework to configure our application's <paramref name="services" />
    /// </summary>
    /// <param name="services">The application's service collection and builders</param>
    public void ConfigureServices(IServiceCollection services)
    {
        // Setup our queue services
        services

            // We only have one queue configured in appsettings.json, lets set it up as the global queue
            .UseGlobalSyncStreamQueueEndpoint(Configuration, "Queue")

            // You can also add it directly from a section of the configuration
            //.UseGlobalSyncStreamQueueEndpoint(Configuration.GetSection("Queue"))

            // As well as directly as a type
            //.UseGlobalSyncStreamQueueEndpoint(Configuration.GetSection("Queue").Get<QueueConfiguration>())

            // We want to use AWS S3 to alias our queue messages for added durability and overhead reduction for
            // RabbitMQ.  This method has the same overloads as UseGlobalSyncStreamQueueEndpoint for adding values
            // from IConfiguration
            .UseGlobalSyncStreamQueueSimpleStorageService(Configuration, "Queue:SimpleStorageService")

            // We'll need a publisher for the queue, so register a scoped service
            .UseSingletonSyncStreamQueuePublisher<ExampleQueueMessage>()

            // We'll also need a subscriber for the queue, let's register it
            // This can also point directly to a static method or an instance method
            .UseSyncStreamQueueSubscriber<ExampleQueueMessage>(HandleQueueMessage);

        // Register our example scoped service
        services
            .AddScoped<ISyncStreamQueueServiceExampleServiceProvider, SyncStreamQueueServiceExampleServiceProvider>();
    }
}
