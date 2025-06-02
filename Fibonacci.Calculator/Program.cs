using Fibonacci.Calculator.Models;
using Fibonacci.Calculator.Services;
using Fibonacci.Calculator.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using StackExchange.Redis;

// Enable HTTP/2 without TLS for local development
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to support HTTP/2 without TLS
builder.WebHost.ConfigureKestrel(options =>
{
    // Listen on localhost:5283 for local dev - HTTP/2 without TLS
    options.ListenAnyIP(5283, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
        listenOptions.UseConnectionLogging();
    });

    // Listen on port 80 for Docker - HTTP/2 without TLS
    options.ListenAnyIP(80, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
        listenOptions.UseConnectionLogging();
    });
});

// Add services to the container.
builder.Services.AddGrpc();

var redisConnectionString = builder.Configuration.GetConnectionString("Redis") 
    ?? throw new InvalidOperationException("Redis connection string is not configured");

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddDbContext<FibonacciDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

// Register RabbitMQ connection factory with configuration
builder.Services.AddSingleton<IConnectionFactory>(sp => 
{
    var config = builder.Configuration.GetSection("RabbitMq");
    return new ConnectionFactory 
    { 
        HostName = config["HostName"] ?? "localhost",
        Port = int.Parse(config["Port"] ?? "5672"),
        UserName = config["UserName"] ?? "guest",
        Password = config["Password"] ?? "guest"
    };
});

// Register application services
builder.Services.AddSingleton<RedisCache>();
builder.Services.AddSingleton<IFibonacciCalculator, FibonacciCalculator>();
builder.Services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<FibonacciService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. " +
"To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

// Test endpoint for debugging - only available over HTTP/1.1 fallback or separate port
// Commenting out since we're now HTTP/2 only
// app.MapGet("/test-db", async (FibonacciDbContext db) =>
// {
//     try
//     {
//         var count = await db.FibonacciResults.CountAsync();
//         return $"✅ DB connected. {count} records.";
//     }
//     catch (Exception ex)
//     {
//         return $"❌ DB failed: {ex.Message}";
//     }
// });

app.Run();
