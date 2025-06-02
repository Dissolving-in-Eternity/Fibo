using System.Text;
using System.Text.Json;
using Fibonacci.Calculator.Services.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Fibonacci.Calculator.Services
{
    /// <summary>
    /// Implementation of message publishing using RabbitMQ.
    /// </summary>
    public class RabbitMqPublisher : IMessagePublisher
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitMqPublisher> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RabbitMqPublisher"/> class.
        /// </summary>
        /// <param name="connectionFactory">The RabbitMQ connection factory</param>
        /// <param name="logger">The logger</param>
        public RabbitMqPublisher(IConnectionFactory connectionFactory, ILogger<RabbitMqPublisher> logger)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Publishes a message to RabbitMQ.
        /// </summary>
        /// <typeparam name="T">The type of message to publish</typeparam>
        /// <param name="message">The message to publish</param>
        /// <param name="exchangeName">The name of the exchange to publish to</param>
        /// <param name="routingKey">The routing key to use</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
        public async Task PublishAsync<T>(T message, string exchangeName, string routingKey)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrEmpty(exchangeName)) throw new ArgumentNullException(nameof(exchangeName));
            if (string.IsNullOrEmpty(routingKey)) throw new ArgumentNullException(nameof(routingKey));

            try
            {
                await using var connection = await _connectionFactory.CreateConnectionAsync();
                await using var channel = await connection.CreateChannelAsync();

                await channel.ExchangeDeclareAsync(exchangeName, "direct", durable: true, autoDelete: false);
                
                var queueName = "factorial.queue";
                await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);
                await channel.QueueBindAsync(queueName, exchangeName, routingKey);

                var messageJson = JsonSerializer.Serialize(message);
                var body = Encoding.UTF8.GetBytes(messageJson);

                var properties = new BasicProperties
                {
                    DeliveryMode = DeliveryModes.Persistent
                };

                await channel.BasicPublishAsync(
                    exchange: exchangeName,
                    routingKey: routingKey,
                    mandatory: false,
                    basicProperties: properties,
                    body: body
                );

                _logger.LogInformation("Published message to exchange {Exchange} with routing key {RoutingKey}", exchangeName, routingKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish message to RabbitMQ");
                throw;
            }
        }
    }
} 