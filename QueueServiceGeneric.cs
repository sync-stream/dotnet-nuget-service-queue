﻿using Microsoft.Extensions.Logging;

// Define our namespace
namespace SyncStream.Service.Queue;

/// <summary>
/// This service is responsible for queueing things
/// </summary>
public class QueueService<TPayload> : QueueService, IQueueService<TPayload>
{
    /// <summary>
    /// This method instantiates our service with a RabbitMQ Connection
    /// </summary>
    /// <param name="logServiceProvider">The log service provider</param>
    /// <param name="defaultEndpoint">Optional default queue endpoint to use</param>
    public QueueService(ILogger<QueueService> logServiceProvider,
        QueueConfiguration defaultEndpoint = null) : base(logServiceProvider, defaultEndpoint)
    {
    }

    /// <summary>
    /// This method instantiates our service with a RabbitMQ Connection
    /// </summary>
    /// <param name="logServiceProvider">The log service provider</param>
    /// <param name="defaultEndpoint">Optional default queue endpoint to use</param>
    public QueueService(ILogger<QueueService> logServiceProvider, string defaultEndpoint = null) : base(
        logServiceProvider, defaultEndpoint)
    {
    }

    /// <summary>
    /// This method publishes a <paramref name="payload"/> to the queue
    /// </summary>
    /// <param name="payload">The message to publish</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    /// <returns>The message that was published</returns>
    public QueueMessage<TPayload> Publish(TPayload payload) => base.Publish(payload);

    /// <summary>
    /// This method publishes a message to <paramref name="queueName" />
    /// </summary>
    /// <param name="queueName">The queue to publish messages to</param>
    /// <param name="payload">The message to publish</param>
    /// <returns>The published message</returns>
    public QueueMessage<TPayload> Publish(string queueName, TPayload payload) => base.Publish(queueName, payload);

    /// <summary>
    /// This method subscribes to the queue
    /// </summary>
    /// <param name="delegateSubscriber">The subscription worker</param>
    public void Subscribe(IQueueService.DelegateSubscriber<TPayload> delegateSubscriber) =>
        base.Subscribe(delegateSubscriber);

    /// <summary>
    /// This method subscribes to <paramref name="queueName" />
    /// </summary>
    /// <param name="queueName">The queue to subscribe to</param>
    /// <param name="delegateSubscriber">The subscription worker</param>
    public void Subscribe(string queueName, IQueueService.DelegateSubscriber<TPayload> delegateSubscriber) =>
        base.Subscribe(queueName, delegateSubscriber);

    /// <summary>
    /// This method asynchronously subscribes to the queue
    /// </summary>
    /// <param name="delegateSubscriber">The message worker</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <returns>An awaitable task containing the message</returns>
    public void Subscribe(IQueueService.DelegateSubscriberAsync<TPayload> delegateSubscriber,
        CancellationToken stoppingToken = default) => base.Subscribe(delegateSubscriber, stoppingToken);

    /// <summary>
    /// This method asynchronously subscribes to <paramref name="queueName" />
    /// </summary>
    /// <param name="queueName">The queue to subscribe to</param>
    /// <param name="delegateSubscriber">The message worker</param>
    /// <param name="stoppingToken">The token denoting task cancellation</param>
    /// <typeparam name="TPayload">The expected message type</typeparam>
    /// <returns>An awaitable task containing the message</returns>
    public void Subscribe(string queueName, IQueueService.DelegateSubscriberAsync<TPayload> delegateSubscriber,
        CancellationToken stoppingToken = default) => base.Subscribe(queueName, delegateSubscriber, stoppingToken);
}
