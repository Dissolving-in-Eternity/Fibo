using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace Fibonacci.Calculator.Services
{
    public class RedisCache
    {
        private readonly IDatabase _db;
        private readonly ILogger<RedisCache> _logger;

        public RedisCache(IConnectionMultiplexer redis, ILogger<RedisCache> logger)
        {
            _db = redis.GetDatabase();
            _logger = logger;
        }

        public async Task<long?> GetFibonacciAsync(int n)
        {
            try
            {
                var value = await _db.StringGetAsync($"fib:{n}");
                if (!value.HasValue || !long.TryParse(value.ToString(), out long result))
                {
                    _logger.LogDebug("Cache miss for Fibonacci({Number})", n);
                    return null;
                }
                
                _logger.LogDebug("Cache hit for Fibonacci({Number})", n);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Fibonacci({Number}) from Redis", n);
                return null;
            }
        }

        public async Task SetFibonacciAsync(int n, long result)
        {
            try
            {
                await _db.StringSetAsync($"fib:{n}", result.ToString(), TimeSpan.FromHours(1));
                _logger.LogDebug("Cached Fibonacci({Number}) in Redis", n);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error caching Fibonacci({Number}) in Redis", n);
            }
        }
    }
}
