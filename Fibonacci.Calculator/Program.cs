using Fibonacci.Calculator.Models;
using Fibonacci.Calculator.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Because I don't want to configure https everywhere and use http for local testing >
builder.WebHost.ConfigureKestrel(options =>
{
    // Listen on localhost:5283 for local dev
    options.ListenAnyIP(5283, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });

    // Listen on port 80 for Docker
    options.ListenAnyIP(80, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

builder.Services.AddDbContext<FibonacciDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddSingleton<RedisCache>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<FibonacciService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. " +
"To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");


//app.MapGet("/test-db", async (FibonacciDbContext db) =>
//{
//    try
//    {
//        var count = await db.FibonacciResults.CountAsync();
//        return $"✅ DB connected. {count} records.";
//    }
//    catch (Exception ex)
//    {
//        return $"❌ DB failed: {ex.Message}";
//    }
//});

app.Run();
