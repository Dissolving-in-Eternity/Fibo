using Fibonacci.Calculator.Models;
using Fibonacci.Calculator.Proto;
using Grpc.Core;

namespace Fibonacci.Calculator.Services
{
    public class FibonacciService : FibonacciCalculator.FibonacciCalculatorBase
    {
        private readonly RedisCache _redis;
        private readonly FibonacciDbContext _db;

        public FibonacciService(RedisCache redis, FibonacciDbContext db)
        {
            _redis = redis;
            _db = db;
        }

        public override async Task<FibonacciReply> Calculate(FibonacciRequest request, ServerCallContext context)
        {
            int n = request.Number;

            // After 92, Fibonacci numbers get bigger than long.MaxValue
            if (n < 0 || n > 92)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Number must be between 0 and 92"));

            // 1. Try Redis
            var redisResult = await _redis.GetFibonacciAsync(n);
            if (redisResult.HasValue)
                return new FibonacciReply { Result = redisResult.Value };

            // 2. Try DB
            var dbResult = await _db.FibonacciResults.FindAsync(n);
            if (dbResult != null)
            {
                await _redis.SetFibonacciAsync(n, dbResult.Result); // cache it
                return new FibonacciReply { Result = dbResult.Result };
            }

            // 3. Calculate
            long result = CalculateFibonacci(n);

            // 4. Store in DB
            _db.FibonacciResults.Add(new FibonacciResult { Number = n, Result = result });
            await _db.SaveChangesAsync();

            // 5. Store in Redis
            await _redis.SetFibonacciAsync(n, result);

            return new FibonacciReply { Result = result };
        }

        private long CalculateFibonacci(int n)
        {
            if (n < 2) return n;

            long a = 0, b = 1;
            for (int i = 2; i <= n; i++)
            {
                long temp = a + b;
                a = b;
                b = temp;
            }
            return b;
        }
    }
}
