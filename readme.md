## SyncStream.Service.Queue
This project contains a collection of library services
for interacting with *RabbitMQ* with optional allowances
for alias messaging with *AWS S3*.

## Installation
```shell
dotnet add package 'SyncStream.Service.Queue'
```

## Alias Messaging with S3
A fancy way of saying that queue messages and their payloads
are stored on AWS S3 as a `json` file.  Once that file has
has been stored, the object key is then sent to RabbitMQ as
opposed to the entire payload.  S3 aliasing is implicitly
enabled by providing a `QueueSimpleStorageServiceConfiguration`
instance.

## Example

##### Queue Configuration C#
```csharp
QueueConfiguration configuration = new QueueConfiguration
{
  Name = "friendly-name",
  
  // Include this to encable encrypted messages on RabbitMQ
  Encryption = new QueueEncryptionConfiguration
  {
      Passes = 1,
      Secret = "sGhA5qK39LkSy85$@R$r%249@*!Rjx3G"
  },
  
  Endpoint = "name-in-rabbit-mq",
  Hostname = "192.168.1.15",
  Secure = false,
  
  // Include this to enable S3 aliasing
  SimpleStorageService = new QueueSimpleStorageServiceConfiguration
  {
    AccessKeyId = "<aws-access_key_id>",
    Bucket = "<bucket-name>",
    
    // Set this to true to encrypt the JSON objects
    // uploaded to S3.  Please keep in mind that, when
    // true, the object contents won't be searchable by
    // Athena
    EncryptObjects = false,
    
    KmsKeyId = "<aws-kms-key-id>",
    Region = "us-east-1",
    SecretAccessKey = "<aws-secret_access_key>"
  },
  
  Password = "SuP3Rs3cR3tP4$5w0rd",
  Username =  "me"
};

```

##### Queue Configuration `appsettings.json`
```json lines
{
  "Queue": {
    "Name": "friendly-name",
    "Encryption": {
      "Passes": 1,
      "Secret": "sGhA5qK39LkSy85$@R$r%249@*!Rjx3G"
    },
    "Endpoint": "name-in-rabbit-mq",
    "Hostname": "192.168.1.1",
    "Secure": false,
    "SimpleStorageService": {
      "AccessKeyId": "<aws-access_key_id>",
      "Bucket": "<bucket-name>",
      "EncryptObjects": false,
      "KmsKeyId": "<aws-kms-key-id>",
      "Region": "us-east-1",
      "SecretAccessKey": "<aws-secret_access_key>"
    },
    "Password": "SuP3Rs3cR3tP4$5w0rd",
    "Username": "me"
  }
}

```

##### SyncStreamQueueServiceExampleMessage.cs
```csharp
// Define our imports
using SyncStream.Service.Queue;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
///  This class maintains the structure of our example queue message
/// </summary>
[XmlRoot("QueueMessage")]
public class ExampleQueueMessage
{
    /// <summary>
    /// This enum maintains our sources for the queue messages
    /// </summary>
    public enum MessageSource
    {
        /// <summary>
        /// This enum value defines the api source
        /// </summary>
        Api,

        /// <summary>
        /// This enum value defines the service-worker source
        /// </summary>
        ServiceWorker,

        /// <summary>
        /// This enum value defines the web-application source
        /// </summary>
        WebApp
    }

    /// <summary>
    /// This property contains the unique publisher ID of the message
    /// </summary>
    [JsonPropertyName("id")]
    [XmlAttribute("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// This property contains the source of the queue message
    /// </summary>
    [JsonPropertyName("source")]
    [XmlAttribute("source")]
    public MessageSource Source { get; set; } = MessageSource.ServiceWorker;

    /// <summary>
    /// This property contains the text to send to the subscriber
    /// </summary>
    [JsonPropertyName("text")]
    [XmlText]
    public string Text { get; set; }

    /// <summary>
    /// This method instantiates a new and empty queue message
    /// </summary>
    public ExampleQueueMessage() { }

    /// <summary>
    /// This method instantiates a new queue message with the <paramref name="text" /> to send
    /// </summary>
    /// <param name="text">The text to send</param>
    public ExampleQueueMessage(string text) => Text = text;

    /// <summary>
    /// This method instantiates a new queue message with a <paramref name="source" /> and optional <paramref name="text" />
    /// </summary>
    /// <param name="source">The MessageSource of the message</param>
    /// <param name="text">Optional, text to send</param>
    public ExampleQueueMessage(MessageSource source = MessageSource.ServiceWorker, string text = null)
    {
        // Set the source of the queue message into the instance
        Source = source;

        // Set the text to send into the instance
        Text = text;
    }
}

```

##### SyncStreamQueueServiceExampleStartup.cs
```csharp
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

             // We want to encrypt the messages going to RabbitMQ
            .UseGlobalSyncStreamQueueEncryption(Configuration, "Queue:Encryption")

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

```
 > *For use with `UseStartup<Startup>()`*

##### SyncStreamQueueServiceExampleServiceProvider.cs
```csharp
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
public class SyncStreamQueueServiceExampleServiceProvider
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

```
