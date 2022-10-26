using System.Security.Authentication;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using SyncStream.Serializer;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
///     This class maintains the structure of a registered queue in the configuration
/// </summary>
[XmlInclude(typeof(QueueSimpleStorageServiceConfiguration))]
[XmlRoot("queueConfiguration")]
public class QueueConfiguration
{
    /// <summary>
    ///     This property contains the encryption configuration for the queue endpoint
    /// </summary>
    [ConfigurationKeyName("encryption")]
    [JsonPropertyName("encryption")]
    [XmlElement("encryption")]
    public QueueServiceEncryptionConfiguration Encryption { get; set; }

    /// <summary>
    ///     This property contains the RabbitMQ name for the queue
    /// </summary>
    [ConfigurationKeyName("endpoint")]
    [JsonPropertyName("endpoint")]
    [XmlAttribute("endpoint")]
    public string Endpoint { get; set; }

    /// <summary>
    ///     This property contains the address at which the RabbitMQ service listens
    /// </summary>
    [ConfigurationKeyName("hostname")]
    [JsonPropertyName("hostname")]
    [XmlAttribute("hostname")]
    public string Hostname { get; set; }

    /// <summary>
    ///     This property contains the friendly name of the queue
    /// </summary>
    [ConfigurationKeyName("name")]
    [JsonPropertyName("name")]
    [XmlAttribute("name")]
    public string Name { get; set; }

    /// <summary>
    ///     This property contains the password with which to authenticate with the RabbitMQ service
    /// </summary>
    [ConfigurationKeyName("password")]
    [JsonPropertyName("password")]
    [XmlAttribute("password")]
    public string Password { get; set; }

    /// <summary>
    ///     This property contains the port number on which the RabbitMQ service listens
    /// </summary>
    [ConfigurationKeyName("port")]
    [JsonPropertyName("port")]
    [XmlAttribute("port")]
    public int Port { get; set; } = 5672;

    /// <summary>
    ///     This property denotes whether the connection to the RabbitMQ service is secure or not
    /// </summary>
    [ConfigurationKeyName("secure")]
    [JsonPropertyName("secure")]
    [XmlAttribute("secure")]
    public bool Secure { get; set; }

    /// <summary>
    ///     This property contains the serialization format to use for queue messages, both in RabbitMQ and AWS S3
    /// </summary>
    [ConfigurationKeyName("serializationFormat")]
    [JsonPropertyName("serializationFormat")]
    [XmlAttribute("serializationFormat")]
    public SerializerFormat SerializationFormat { get; set; } = SerializerFormat.Json;

    /// <summary>
    ///     This property contains the AWS S3 configuration for the queue
    /// </summary>
    [ConfigurationKeyName("simpleStorageService")]
    [JsonPropertyName("simpleStorageService")]
    [XmlElement("SimpleStorageService")]
    public QueueSimpleStorageServiceConfiguration SimpleStorageService { get; set; }

    /// <summary>
    ///     This property denotes whether logs should be suppressed or not
    /// </summary>
    [ConfigurationKeyName("suppressLog")]
    [JsonPropertyName("suppressLog")]
    [XmlAttribute("suppressLog")]
    public bool SuppressLog { get; set; }

    /// <summary>
    ///     This property contains the username with which to authenticate with the RabbitMQ service
    /// </summary>
    [ConfigurationKeyName("username")]
    [JsonPropertyName("username")]
    [XmlAttribute("username")]
    public string Username { get; set; }

    /// <summary>
    ///     This property contains the virtual host on which the queue lives inside the RabbitMQ service
    /// </summary>
    [ConfigurationKeyName("virtualHost")]
    [JsonPropertyName("virtualHost")]
    [XmlAttribute("vhost")]
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    ///     This property contains the connected channel to the queue
    /// </summary>
    [JsonIgnore]
    [XmlIgnore]
    public IModel Channel { get; private set; }

    /// <summary>
    ///     This property contains the connection to the queue
    /// </summary>
    [JsonIgnore]
    [XmlIgnore]
    public IConnection Connection { get; private set; }

    /// <summary>
    ///     This method disconnects from the queue endpoint
    /// </summary>
    /// <returns>This instance</returns>
    public QueueConfiguration Disconnect()
    {
        // Close the channel
        Channel?.Close();

        // Close the connection
        Connection?.Close();

        // We're done, return the instance
        return this;
    }

    /// <summary>
    ///     This method returns the connected RabbitMQ channel from the instance
    /// </summary>
    /// <returns>The connected RabbitMQ channel</returns>
    public IModel GetChannel()
    {
        // Check for an existing channel and return it
        if (Channel is not null) return Channel;

        // Generate the new channel
        Channel = NewChannel();

        // Declare our queue
        Channel.QueueDeclarePassive(Endpoint);

        // Set our quality of service
        Channel.BasicQos(0, 1, false);

        // We're done, return the channel
        return Channel;
    }

    /// <summary>
    ///     This method returns the RabbitMQ connection from the instance
    /// </summary>
    /// <returns>The RabbitMQ connection</returns>
    public IConnection GetConnection()
    {
        // Check for an existing connection
        if (Connection is not null && Connection.IsOpen) return Connection;

        // Generate a new connection for the instance
        Connection = NewConnection();

        // We're done, return the connection from the instance
        return Connection;
    }

    /// <summary>
    ///     This method generates a new connected RabbitMQ channel from the instance connection
    /// </summary>
    /// <returns>The connected RabbitMQ channel</returns>
    public IModel NewChannel() => GetConnection().CreateModel();

    /// <summary>
    ///     This method generates a new connection from the instance
    /// </summary>
    /// <returns>The new RabbitMQ IConnection</returns>
    public IConnection NewConnection() => new ConnectionFactory()
    {
        // Define our AMQP TLS protocol
        AmqpUriSslProtocols = SslProtocols.Tls12,

        // Dispatch the consumers asynchronously
        DispatchConsumersAsync = true,

        // Set the host name of our queue server into the connection factory
        HostName = Hostname,

        // Set the password for our queue server into the connection factory
        Password = Password,

        // Set the port of our queue server into the connection factory
        Port = Port,

        // Set the SSL settings of our queue server into the connection factory
        Ssl = new()
        {
            // Set the SSL flag into the connection factory
            Enabled = Secure,

            // Set the SSL server name into the connection factory
            ServerName = Hostname,
        },

        // We want to use background threads for I/O
        UseBackgroundThreadsForIO = true,

        // Set the username for our queue server into the connection factory
        UserName = Username,

        // Set the virtual host of our queue server into the connection factory
        VirtualHost = VirtualHost
    }.CreateConnection();

    /// <summary>
    ///     This method generates an amqp URI from the instance
    /// </summary>
    /// <returns>The amqp URI</returns>
    public Uri ToUri() => new($"{(Secure ? "amqps" : "amqp")}://${Hostname}:{Port}", UriKind.Absolute);
}
