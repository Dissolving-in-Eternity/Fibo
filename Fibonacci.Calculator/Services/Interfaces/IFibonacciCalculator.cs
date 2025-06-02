namespace Fibonacci.Calculator.Services.Interfaces
{
    /// <summary>
    /// Interface for calculating Fibonacci numbers.
    /// </summary>
    public interface IFibonacciCalculator
    {
        /// <summary>
        /// Calculates the nth Fibonacci number.
        /// </summary>
        /// <param name="n">The position in the Fibonacci sequence (0-based)</param>
        /// <returns>The nth Fibonacci number</returns>
        /// <exception cref="ArgumentException">Thrown when n is negative</exception>
        /// <exception cref="OverflowException">Thrown when the result exceeds long.MaxValue</exception>
        long Calculate(int n);
    }
} 