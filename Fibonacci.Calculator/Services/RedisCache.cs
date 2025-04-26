using StackExchange.Redis;

namespace Fibonacci.Calculator.Services
{
    public class RedisCache
    {
        private readonly IDatabase _db;

        public RedisCache(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task<long?> GetFibonacciAsync(int n)
        {
            var value = await _db.StringGetAsync($"fib:{n}");
            return value.HasValue ? (long?)long.Parse(value) : null;
        }

        public Task SetFibonacciAsync(int n, long result)
        {
            return _db.StringSetAsync($"fib:{n}", result, TimeSpan.FromHours(1));
        }
    }
}
