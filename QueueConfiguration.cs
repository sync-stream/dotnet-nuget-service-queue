using System.Net.Security;
using System.Security.Authentication;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using RabbitMQ.Client;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This class maintains the structure of a registered queue in the configuration
/// </summary>
[XmlInclude(typeof(QueueSimpleStorageServiceConfiguration))]
[XmlRoot("QueueConfiguration")]
public class QueueConfiguration
{
    /// <summary>
    /// This property contains the encryption configuration for the queue endpoint
    /// </summary>
    [JsonPropertyName("Encryption")]
    [XmlElement("Encryption")]
    public QueueServiceEncryptionConfiguration Encryption { get; set; }

        /// <summary>
    /// This property contains the RabbitMQ name for the queue
    /// </summary>
    [JsonPropertyName("Endpoint")]
    [XmlAttribute("endpoint")]
    public string Endpoint { get; set; }

    /// <summary>
    /// This property contains the address at which the RabbitMQ service listens
    /// </summary>
    [JsonPropertyName("Hostname")]
    [XmlAttribute("hostname")]
    public string Hostname { get; set; }

    /// <summary>
    /// This property contains the friendly name of the queue
    /// </summary>
    [JsonPropertyName("Name")]
    [XmlAttribute("name")]
    public string Name { get; set; }

    /// <summary>
    /// This property contains the password with which to authenticate with the RabbitMQ service
    /// </summary>
    [JsonPropertyName("Password")]
    [XmlAttribute("password")]
    public string Password { get; set; }

    /// <summary>
    /// This property contains the port number on which the RabbitMQ service listens
    /// </summary>
    [JsonPropertyName("Port")]
    [XmlAttribute("port")]
    public int Port { get; set; } = 5672;

    /// <summary>
    /// This property denotes whether the connection to the RabbitMQ service is secure or not
    /// </summary>
    [JsonPropertyName("Secure")]
    [XmlAttribute("secure")]
    public bool Secure { get; set; }

    /// <summary>
    /// This property contains the AWS S3 configuration for the queue
    /// </summary>
    [JsonPropertyName("SimpleStorageService")]
    [XmlElement("SimpleStorageService")]
    public QueueSimpleStorageServiceConfiguration SimpleStorageService { get; set; }

    /// <summary>
    /// This property contains the username with which to authenticate with the RabbitMQ service
    /// </summary>
    [JsonPropertyName("Username")]
    [XmlAttribute("username")]
    public string Username { get; set; }

    /// <summary>
    /// This property contains the virtual host on which the queue lives inside the RabbitMQ service
    /// </summary>
    [JsonPropertyName("VirtualHost")]
    [XmlAttribute("vhost")]
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    ///  This property contains the connected channel to the queue
    /// </summary>
    [JsonIgnore]
    [XmlIgnore]
    public IModel Channel { get; private set; }

    /// <summary>
    /// This property contains the connection to the queue
    /// </summary>
    [JsonIgnore]
    [XmlIgnore]
    public IConnection Connection { get; private set; }

    /// <summary>
    /// This method disconnects from the queue endpoint
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
    /// This method returns the connected RabbitMQ channel from the instance
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
    /// This method returns the RabbitMQ connection from the instance
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
    /// This method generates a new connected RabbitMQ channel from the instance connection
    /// </summary>
    /// <returns>The connected RabbitMQ channel</returns>
    public IModel NewChannel() =>
        GetConnection()?.CreateModel();

    /// <summary>
    /// This method generates a new connection from the instance
    /// </summary>
    /// <returns>The new RabbitMQ IConnection</returns>
    public IConnection NewConnection() =>
        new ConnectionFactory()
        {
            // Dispatch the consumers asynchronously
            DispatchConsumersAsync = true,

            // Set the host name of our queue server into the connection factory
            HostName = Hostname,

            // Set the password for our queue server into the connection factory
            Password = Password,

            // Set the port of our queue server into the connection factory
            Port = Port,

            // Set the SSL settings of our queue server into the connection factory
            Ssl = Secure
                ? new()
                {
                    // Set the acceptable SSL policy errors into the connection factory
                    AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch,

                    // Set the SSL flag into the connection factory
                    Enabled = Secure,

                    // Set the SSL server name into the connection factory
                    ServerName = "rmq",

                    // Set the SSL version into the connection factory
                    Version = SslProtocols.Tls12
                }
                : null,

            // Set the username for our queue server into the connection factory
            UserName = Username,

            // Set the virtual host of our queue server into the connection factory
            VirtualHost = VirtualHost
        }.CreateConnection();
}
