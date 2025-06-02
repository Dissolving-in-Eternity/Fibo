using Fibonacci.Calculator.Services.Interfaces;

namespace Fibonacci.Calculator.Services
{
    /// <summary>
    /// Simple implementation of Fibonacci number calculation using iterative approach.
    /// </summary>
    public class FibonacciCalculator : IFibonacciCalculator
    {
        /// <summary>
        /// Calculates the nth Fibonacci number using a simple iterative approach.
        /// </summary>
        /// <param name="n">The position in the Fibonacci sequence (0-based)</param>
        /// <returns>The nth Fibonacci number</returns>
        /// <exception cref="ArgumentException">Thrown when n is negative</exception>
        /// <exception cref="OverflowException">Thrown when the result exceeds long.MaxValue</exception>
        public long Calculate(int n)
        {
            if (n < 0)
                throw new ArgumentException("Fibonacci number cannot be negative", nameof(n));

            if (n < 2) return n;

            long a = 0, b = 1;
            for (int i = 2; i <= n; i++)
            {
                long temp = checked(a + b);
                a = b;
                b = temp;
            }

            return b;
        }
    }
} 