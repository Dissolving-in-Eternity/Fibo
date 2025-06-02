namespace Fibonacci.Calculator.Services.Interfaces
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T message, string exchangeName, string routingKey);
    }
} 