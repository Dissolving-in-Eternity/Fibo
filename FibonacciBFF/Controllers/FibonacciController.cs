using Fibonacci.Calculator.Proto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FibonacciBFF.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FibonacciController : ControllerBase
    {
        private readonly FibonacciCalculator.FibonacciCalculatorClient _grpcClient;

        public FibonacciController(FibonacciCalculator.FibonacciCalculatorClient grpcClient)
        {
            _grpcClient = grpcClient;
        }

        [HttpGet("{number}")]
        public async Task<IActionResult> GetFibonacci(int number)
        {
            var request = new FibonacciRequest { Number = number };
            var response = await _grpcClient.CalculateAsync(request);

            return Ok(response.Result);
        }
    }
}
