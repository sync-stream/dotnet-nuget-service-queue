using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;
using SyncStream.Cryptography;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This class maintains the structure of an encrypted queue message
/// </summary>
/// <typeparam name="TEncryptedPayload"></typeparam>
[XmlInclude(typeof(QueueMessage<>))]
[XmlRoot("encryptedQueueMessage")]
public class EncryptedQueueMessage<TEncryptedPayload> : QueueMessage<string>
{
    /// <summary>
    ///     This method decrypts the <paramref name="hash" /> into a string
    /// </summary>
    /// <param name="hash">The hash to decrypt</param>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <returns>The decrypted <paramref name="hash" /></returns>
    public static string Decrypt(string hash, QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        CryptographyService.Decrypt(hash, encryptionConfiguration);

    /// <summary>
    ///     This method decrypts <paramref name="hash" />
    ///     and returns the <typeparamref name="TEncryptedPayload" /> object
    /// </summary>
    /// <param name="hash">The encrypted hash</param>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <typeparam name="TOutput">The expected type of the output object</typeparam>
    /// <returns>The decrypted <typeparamref name="TEncryptedPayload" /> object</returns>
    public static TOutput Decrypt<TOutput>(string hash, QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        typeof(TOutput) == typeof(string)
            ? (TOutput)Convert.ChangeType(Decrypt(hash, encryptionConfiguration), typeof(TOutput))
            : CryptographyService.Decrypt<TOutput>(hash, encryptionConfiguration);

    /// <summary>
    ///     This method asynchronously decrypts the <paramref name="hash" /> into a string
    /// </summary>
    /// <param name="hash">The hash to decrypt</param>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <returns>An awaitable task containing the decrypted <paramref name="hash" /></returns>
    public static Task<string> DecryptAsync(string hash, QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        CryptographyService.DecryptAsync(hash, encryptionConfiguration);

    /// <summary>
    ///     This method asynchronously decrypts <paramref name="hash" />
    ///     and returns the <typeparamref name="TEncryptedPayload" /> object
    /// </summary>
    /// <param name="hash">The encrypted hash</param>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <typeparam name="TOutput">The expected type of the output object</typeparam>
    /// <returns>An awaitable task containing the decrypted <typeparamref name="TEncryptedPayload" /> object</returns>
    public static async Task<TOutput>
        DecryptAsync<TOutput>(string hash, QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        typeof(TOutput) == typeof(string)
            ? (TOutput)Convert.ChangeType(await DecryptAsync(hash, encryptionConfiguration), typeof(TOutput))
            : await CryptographyService.DecryptAsync<TOutput>(hash, encryptionConfiguration);

    /// <summary>
    ///     This method provides encryption for <paramref name="value" /> of <typeparamref name="TInput" />
    /// </summary>
    /// <param name="value">The value to be encrypted</param>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <typeparam name="TInput">The expected type of <paramref name="value" /></typeparam>
    /// <returns>The portable cryptographic hash of <paramref name="value" /></returns>
    public static string Encrypt<TInput>(TInput value, QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        typeof(TInput) == typeof(string)
            ? CryptographyService.Encrypt(value as string, encryptionConfiguration)
            : CryptographyService.Encrypt(value, encryptionConfiguration);

    /// <summary>
    ///     This method provides asynchronous encryption for <paramref name="value" /> of <typeparamref name="TInput" />
    /// </summary>
    /// <param name="value">The value to be encrypted</param>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <typeparam name="TInput">The expected type of <paramref name="value" /></typeparam>
    /// <returns>An awaitable task containing the portable cryptographic hash of <paramref name="value" /></returns>
    public static Task<string>
        EncryptAsync<TInput>(TInput value, QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        typeof(TInput) == typeof(string)
            ? CryptographyService.EncryptAsync(value as string, encryptionConfiguration)
            : CryptographyService.EncryptAsync(value, encryptionConfiguration);

    /// <summary>
    /// This property contains the payload for the Queue Message
    /// </summary>
    [JsonPropertyName("payload")]
    [XmlElement("payload")]
    public new string Payload { get; set; }

    /// <summary>
    /// This method instantiates an empty encrypted message
    /// </summary>
    public EncryptedQueueMessage()
    {
    }

    /// <summary>
    ///     This method instantiates an encrypted queue message from a <paramref name="payload" />
    /// </summary>
    /// <param name="payload">The payload to encrypt</param>
    /// <param name="encryptionConfiguration">The queue encryption configuration</param>
    public EncryptedQueueMessage(string payload,
        QueueServiceEncryptionConfiguration encryptionConfiguration) : base(Encrypt(payload, encryptionConfiguration))
    {
    }

    /// <summary>
    ///     This method instantiates an encrypted message from an existing <paramref name="message" />
    /// </summary>
    /// <param name="message">The existing queue message</param>
    /// <param name="encryptionConfiguration">The queue encryption configuration </param>
    public EncryptedQueueMessage(QueueMessage<TEncryptedPayload> message,
        QueueServiceEncryptionConfiguration encryptionConfiguration)
    {
        // Set the consumed timestamp into the instance
        Consumed = message.Consumed;

        // Set the created timestamp into the instance
        Created = message.Created;

        // Set the unique ID of the message into the instance
        Id = message.Id;

        // Set the published timestamp into the instance
        Published = message.Published;

        // Encrypt the payload and set it into the instance
        WithPayload(message.Payload, encryptionConfiguration);
    }

    /// <summary>
    ///     This method instantiates a new encrypted queue message with a
    ///     <paramref name="payload" /> from the <paramref name="section" />
    ///     of the application's <paramref name="configuration" />
    /// </summary>
    /// <param name="payload">The payload to be encrypted and sent to the queue</param>
    /// <param name="configuration">The application's configuration provider</param>
    /// <param name="section">The section of the application's configuration containing the encryption values</param>
    public EncryptedQueueMessage(string payload, IConfiguration configuration, string section) :
        this(payload, configuration.GetSection(section).Get<QueueServiceEncryptionConfiguration>())
    {
    }

    /// <summary>
    ///     This method instantiates a new encrypted queue message with a
    ///     <paramref name="payload" /> from the <paramref name="section" />
    ///     of the application's configuration
    /// </summary>
    /// <param name="payload">The payload to be encrypted and sent to the queue</param>
    /// <param name="section">The section of the application's configuration containing the encryption values</param>
    public EncryptedQueueMessage(string payload, IConfigurationSection section) : this(payload,
        section.Get<QueueServiceEncryptionConfiguration>())
    {
    }

    /// <summary>
    ///     This method instantiates a new encrypted queue message with a
    ///     <paramref name="payload" /> from the <paramref name="secret" />
    ///     and optional number of recursive encryption <paramref name="passes" />
    /// </summary>
    /// <param name="payload">The payload to be encrypted and sent to the queue</param>
    /// <param name="secret">The secret key used for encryption and decryption</param>
    /// <param name="passes">Optional, number of times to recursively encrypt the data</param>
    public EncryptedQueueMessage(string payload, string secret, int passes = 1) : this(payload,
        new QueueServiceEncryptionConfiguration(secret, passes))
    {
    }

    /// <summary>
    ///     This method decrypts and returns the payload from the instance
    /// </summary>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <returns>The decrypted payload from the instance</returns>
    public TEncryptedPayload GetPayload(QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        Decrypt<TEncryptedPayload>(Payload, encryptionConfiguration);


    /// <summary>
    ///     This method asynchronously decrypts and returns the payload from the instance
    /// </summary>
    /// <param name="encryptionConfiguration">The cryptographic settings</param>
    /// <returns>An awaitable task containing the decrypted payload from the instance</returns>
    public Task<TEncryptedPayload> GetPayloadAsync(QueueServiceEncryptionConfiguration encryptionConfiguration) =>
        DecryptAsync<TEncryptedPayload>(Payload, encryptionConfiguration);

    /// <summary>
    ///     This method decrypts the instance into a standard queue message
    /// </summary>
    /// <param name="encryptionConfiguration">The cryptographic configuration</param>
    /// <returns>The decrypted instance</returns>
    public QueueMessage<TEncryptedPayload>
        ToQueueMessage(QueueServiceEncryptionConfiguration encryptionConfiguration) => new()
    {
        // Set the consumed timestamp into the instance
        Consumed = Consumed,

        // Set the created timestamp into the instance
        Created = Created,

        // Set the unique ID of the message into the instance
        Id = Id,

        // Encrypt the payload and set the hash into the instance
        Payload = Decrypt<TEncryptedPayload>(Payload, encryptionConfiguration),

        // Set the published timestamp into the instance
        Published = Published
    };

    /// <summary>
    ///     This method asynchronously decrypts the instance into a standard queue message
    /// </summary>
    /// <param name="encryptionConfiguration">The cryptographic configuration</param>
    /// <returns>An awaitable task containing the decrypted instance</returns>
    public async Task<QueueMessage<TEncryptedPayload>> ToQueueMessageAsync(
        QueueServiceEncryptionConfiguration encryptionConfiguration) => new()
    {
        // Set the consumed timestamp into the instance
        Consumed = Consumed,

        // Set the created timestamp into the instance
        Created = Created,

        // Set the unique ID of the message into the instance
        Id = Id,

        // Encrypt the payload and set the hash into the instance
        Payload = await DecryptAsync<TEncryptedPayload>(Payload, encryptionConfiguration),

        // Set the published timestamp into the instance
        Published = Published
    };

    /// <summary>
    ///     This method encrypts a new payload and sets it into the instance
    /// </summary>
    /// <param name="payload">The new payload to encrypt</param>
    /// <param name="encryptionConfiguration">The queue encryption configuration values</param>
    /// <returns>The current instance with the encrypted payload</returns>
    public EncryptedQueueMessage<TEncryptedPayload> WithPayload(TEncryptedPayload payload,
        QueueServiceEncryptionConfiguration encryptionConfiguration)
    {
        // Asynchronously encrypt the payload into the instance
        Payload = Encrypt(payload, encryptionConfiguration);

        // We're done, return the instance
        return this;
    }

    /// <summary>
    ///     This method encrypts <paramref name="payload" /> from the
    ///     <paramref name="section" /> of the application's <paramref name="configuration" />
    /// </summary>
    /// <param name="payload">The payload to asynchronously encrypt</param>
    /// <param name="configuration">The application's configuration provider</param>
    /// <param name="section">The section of the application's configuration containing the encryption values</param>
    /// <returns>The current instance with the encrypted payload</returns>
    public EncryptedQueueMessage<TEncryptedPayload>
        WithPayload(TEncryptedPayload payload, IConfiguration configuration, string section) => WithPayload(payload,
        configuration.GetSection(section).Get<QueueServiceEncryptionConfiguration>());

    /// <summary>
    ///     This method encrypts <paramref name="payload" />
    ///     from the <paramref name="section" /> of the application's configuration
    /// </summary>
    /// <param name="payload">The payload to asynchronously encrypt</param>
    /// <param name="section">The section of the application's configuration containing the encryption values</param>
    /// <returns>An awaitable task containing the current instance with the encrypted payload</returns>
    public EncryptedQueueMessage<TEncryptedPayload> WithPayload(TEncryptedPayload payload,
        IConfigurationSection section) =>
        WithPayload(payload, section.Get<QueueServiceEncryptionConfiguration>());

    /// <summary>
    ///     This method encrypts the <paramref name="payload" />
    ///     using the <paramref name="secret"/> with an optional number of
    ///     recursive encryption <paramref name="passes" />
    /// </summary>
    /// <param name="payload">The new payload to asynchronously encrypt</param>
    /// <param name="secret">The cryptographic secret to use</param>
    /// <param name="passes">Optional, number of times to recursively encrypt the data</param>
    /// <returns>The current instance with the encrypted payload</returns>
    public EncryptedQueueMessage<TEncryptedPayload> WithPayload(TEncryptedPayload payload, string secret,
        int passes = 1) =>
        WithPayload(payload, new QueueServiceEncryptionConfiguration(secret, passes));

    /// <summary>
    ///     This method asynchronously encrypts a new payload and sets it into the instance
    /// </summary>
    /// <param name="payload">The new payload to encrypt</param>
    /// <param name="encryptionConfiguration">The queue encryption configuration values</param>
    /// <returns>An awaitable task containing the current instance with the encrypted payload</returns>
    public async Task<EncryptedQueueMessage<TEncryptedPayload>> WithPayloadAsync(TEncryptedPayload payload,
        QueueServiceEncryptionConfiguration encryptionConfiguration)
    {
        // Asynchronously encrypt the payload into the instance
        Payload = (typeof(TEncryptedPayload) == typeof(string))
            ? await EncryptAsync(payload as string, encryptionConfiguration)
            : await EncryptAsync(payload, encryptionConfiguration);

        // We're done, return the instance
        return this;
    }

    /// <summary>
    ///     This method asynchronously encrypts <paramref name="payload" /> from the
    ///     <paramref name="section" /> of the application's <paramref name="configuration" />
    /// </summary>
    /// <param name="payload">The payload to asynchronously encrypt</param>
    /// <param name="configuration">The application's configuration provider</param>
    /// <param name="section">The section of the application's configuration containing the encryption values</param>
    /// <returns>An awaitable task containing the current instance with the encrypted payload</returns>
    public Task<EncryptedQueueMessage<TEncryptedPayload>>
        WithPayloadAsync(TEncryptedPayload payload, IConfiguration configuration, string section) => WithPayloadAsync(
        payload,
        configuration.GetSection(section).Get<QueueServiceEncryptionConfiguration>());

    /// <summary>
    ///     This method asynchronously encrypts <paramref name="payload" />
    ///     from the <paramref name="section" /> of the application's configuration
    /// </summary>
    /// <param name="payload">The payload to asynchronously encrypt</param>
    /// <param name="section">The section of the application's configuration containing the encryption values</param>
    /// <returns>An awaitable task containing the current instance with the encrypted payload</returns>
    public Task<EncryptedQueueMessage<TEncryptedPayload>> WithPayloadAsync(TEncryptedPayload payload,
        IConfigurationSection section) =>
        WithPayloadAsync(payload, section.Get<QueueServiceEncryptionConfiguration>());

    /// <summary>
    ///     This method asynchronously encrypts the <paramref name="payload" />
    ///     using the <paramref name="secret"/> with an optional number of
    ///     recursive encryption <paramref name="passes" />
    /// </summary>
    /// <param name="payload">The new payload to asynchronously encrypt</param>
    /// <param name="secret">The cryptographic secret to use</param>
    /// <param name="passes">Optional, number of times to recursively encrypt the data</param>
    /// <returns>An awaitable task containing the current instance with the encrypted payload</returns>
    public Task<EncryptedQueueMessage<TEncryptedPayload>> WithPayloadAsync(TEncryptedPayload payload, string secret,
        int passes = 1) =>
        WithPayloadAsync(payload, new QueueServiceEncryptionConfiguration(secret, passes));
}
