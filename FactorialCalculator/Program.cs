using FactorialCalculator;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<FactorialConsumerService>();
    })
    .Build();

host.Run();
