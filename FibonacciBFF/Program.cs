using Fibonacci.Calculator.Proto;
using System.Net;

// Enable HTTP/2 without TLS for local development
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddGrpcClient<FibonacciCalculator.FibonacciCalculatorClient>(o =>
{
    // Prioritize environment variable, then configuration, then local development fallback
    var grpcServiceUrl = Environment.GetEnvironmentVariable("GRPC_SERVICE_URL")
                         ?? builder.Configuration["GrpcServiceUrl"] 
                         ?? "http://localhost:5283"; // Local development fallback
    
    o.Address = new Uri(grpcServiceUrl);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    return handler;
})
.ConfigureChannel(options =>
{
    // Configure for HTTP/2 without TLS
    options.HttpVersion = new Version(2, 0);
    options.ThrowOperationCanceledOnCancellation = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fibonacci BFF V1");
    c.RoutePrefix = "";
});

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
