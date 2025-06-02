using System.Text.Json;
using System.Text;
using Fibonacci.Calculator.Models;
using Fibonacci.Calculator.Proto;
using Fibonacci.Calculator.Services.Interfaces;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Fibonacci.Calculator.Services
{
    /// <summary>
    /// gRPC service implementation for calculating Fibonacci numbers.
    /// Handles caching, persistence, and message publishing for factorial calculations.
    /// </summary>
    public class FibonacciService : Proto.FibonacciCalculator.FibonacciCalculatorBase
    {
        private const string ExchangeName = "factorial.calculations";
        private const string RoutingKey = "fib.result";

        private readonly RedisCache _redis;
        private readonly FibonacciDbContext _db;
        private readonly IFibonacciCalculator _calculator;
        private readonly IMessagePublisher _publisher;
        private readonly ILogger<FibonacciService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FibonacciService"/> class.
        /// </summary>
        /// <param name="redis">Redis cache service for storing Fibonacci results</param>
        /// <param name="db">Database context for persisting Fibonacci results</param>
        /// <param name="calculator">Service for calculating Fibonacci numbers</param>
        /// <param name="publisher">Service for publishing messages to RabbitMQ</param>
        /// <param name="logger">Logger for the service</param>
        public FibonacciService(
            RedisCache redis, 
            FibonacciDbContext db,
            IFibonacciCalculator calculator,
            IMessagePublisher publisher,
            ILogger<FibonacciService> logger)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Calculates the Fibonacci number for the given input.
        /// Implements caching strategy: Redis -> Database -> Calculation.
        /// </summary>
        /// <param name="request">The Fibonacci calculation request</param>
        /// <param name="context">The gRPC call context</param>
        /// <returns>The calculated Fibonacci number</returns>
        /// <exception cref="RpcException">Thrown when the input is invalid or calculation fails</exception>
        public override async Task<FibonacciReply> Calculate(FibonacciRequest request, ServerCallContext context)
        {
            try
            {
                int n = request.Number;

                if (n < 0)
                {
                    _logger.LogWarning("Invalid input: negative number {Number}", n);
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Number cannot be negative"));
                }

                // 1. Try Redis
                var redisResult = await _redis.GetFibonacciAsync(n);
                if (redisResult.HasValue)
                {
                    _logger.LogInformation("Cache hit in Redis for Fibonacci({Number})", n);
                    return new FibonacciReply { Result = redisResult.Value };
                }

                // 2. Try DB
                var dbResult = await _db.FibonacciResults.FindAsync(n);
                if (dbResult != null)
                {
                    _logger.LogInformation("Cache hit in DB for Fibonacci({Number})", n);
                    await _redis.SetFibonacciAsync(n, dbResult.Result); // cache it
                    return new FibonacciReply { Result = dbResult.Result };
                }

                // 3. Calculate
                _logger.LogInformation("Calculating Fibonacci({Number})", n);
                var result = _calculator.Calculate(n);

                // 4. Store in DB
                _db.FibonacciResults.Add(new FibonacciResult { Number = n, Result = result });
                await _db.SaveChangesAsync();

                // 5. Store in Redis
                await _redis.SetFibonacciAsync(n, result);

                // 6. Publish to queue for factorial calculation - using correct exchange and routing key
                await _publisher.PublishAsync(
                    new { Number = n, FibonacciResult = result },
                    ExchangeName,
                    RoutingKey
                );

                _logger.LogInformation("Successfully calculated and published Fibonacci({Number}) for factorial calculation", n);
                return new FibonacciReply { Result = result };
            }
            catch (OverflowException ex)
            {
                _logger.LogError(ex, "Overflow calculating Fibonacci({Number})", request.Number);
                throw new RpcException(new Status(StatusCode.OutOfRange, "Result is too large"));
            }
            catch (Exception ex) when (ex is not RpcException)
            {
                _logger.LogError(ex, "Error calculating Fibonacci({Number})", request.Number);
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
            }
        }
    }
}
