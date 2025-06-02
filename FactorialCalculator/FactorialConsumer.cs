using System.Numerics;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FactorialCalculator
{
    public class FactorialRequest
    {
        public int Number { get; set; }
        public long FibonacciResult { get; set; }
    }

    public class FactorialConsumerService : BackgroundService
    {
        private readonly ILogger<FactorialConsumerService> _logger;
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IChannel? _channel;

        public FactorialConsumerService(ILogger<FactorialConsumerService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await InitializeRabbitMqAsync();
                
                _logger.LogInformation("Factorial Consumer Service started at: {time}", DateTimeOffset.Now);

                // Keep the service running
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Factorial Consumer Service");
                throw;
            }
        }

        private async Task InitializeRabbitMqAsync()
        {
            var rabbitMqConfig = _configuration.GetSection("RabbitMq");
            var exchangeConfig = _configuration.GetSection("RabbitMqExchange");

            var factory = new ConnectionFactory
            {
                HostName = rabbitMqConfig["HostName"] ?? "localhost",
                Port = int.Parse(rabbitMqConfig["Port"] ?? "5672"),
                UserName = rabbitMqConfig["UserName"] ?? "guest",
                Password = rabbitMqConfig["Password"] ?? "guest"
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            // Declare exchange
            var exchangeName = "factorial.calculations";
            var exchangeType = exchangeConfig["Type"] ?? "direct";
            var durable = bool.Parse(exchangeConfig["Durable"] ?? "true");
            var autoDelete = bool.Parse(exchangeConfig["AutoDelete"] ?? "false");

            await _channel.ExchangeDeclareAsync(exchangeName, exchangeType, durable, autoDelete);

            // Declare queue
            var queueName = "factorial.queue";
            await _channel.QueueDeclareAsync(queueName, durable, false, autoDelete);

            // Bind queue to exchange
            var routingKey = "fib.result";
            await _channel.QueueBindAsync(queueName, exchangeName, routingKey);

            // Set up consumer
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    
                    await ProcessMessageAsync(message);
                    
                    // Acknowledge the message
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");
                    // Reject the message and don't requeue it
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            await _channel.BasicConsumeAsync(queueName, false, consumer);
            
            _logger.LogInformation("RabbitMQ consumer initialized and listening on queue: {QueueName}", queueName);
        }

        private async Task ProcessMessageAsync(string message)
        {
            try
            {
                _logger.LogInformation("Processing message: {Message}", message);
                
                var factorialRequest = JsonSerializer.Deserialize<FactorialRequest>(message);

                if (factorialRequest == null)
                {
                    _logger.LogError("Failed to deserialize message: {Message}", message);
                    return;
                }

                _logger.LogInformation("Deserialized message - Number: {Number}, FibonacciResult: {FibonacciResult}", 
                    factorialRequest.Number, factorialRequest.FibonacciResult);

                if (factorialRequest.Number < 0)
                {
                    _logger.LogWarning("Received negative number {Number}, returning 0", factorialRequest.Number);
                    return;
                }

                var result = await Task.Run(() => Factorial(factorialRequest.FibonacciResult));
                _logger.LogInformation("Calculated factorial of Fibonacci result {FibonacciResult} = {Result}", 
                    factorialRequest.FibonacciResult, result);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize message: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing factorial calculation");
            }
        }

        private BigInteger Factorial(long n)
        {
            if (n == 0 || n == 1)
                return 1;

            BigInteger result = 1;
            for (int i = 2; i <= n; i++)
                result *= i;
            return result;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Factorial Consumer Service is stopping.");

            if (_channel != null)
            {
                await _channel.CloseAsync();
                _channel.Dispose();
            }

            if (_connection != null)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
            }

            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
} 