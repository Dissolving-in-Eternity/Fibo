namespace Fibonacci.Calculator.Models
{
    /// <summary>
    /// Represents a Fibonacci calculation result.
    /// </summary>
    public class FibonacciResult
    {
        /// <summary>
        /// The input number for which the Fibonacci value was calculated.
        /// This is the user-provided number, not an auto-generated ID.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// The calculated Fibonacci value for the input number.
        /// </summary>
        public long Result { get; set; }
    }
}
