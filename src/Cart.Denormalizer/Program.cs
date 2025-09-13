using Cart.Denormalizer;
using StackExchange.Redis;
using Shared.Kernel;
using Shared.Kernel.Kafka;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddKafkaServices(context.Configuration);
        services.AddKafkaConsumer(context.Configuration, "cart-denormalizer-group");

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionString = context.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
            var configuration = ConfigurationOptions.Parse(connectionString);
            return ConnectionMultiplexer.Connect(configuration);
        });

        services.AddHostedService<Worker>();
    })
    .Build();

using var scope = host.Services.CreateScope();
var topicManager = scope.ServiceProvider.GetRequiredService<ITopicManager>();
await topicManager.EnsureTopicsExistAsync(["cart-events"]);

await host.RunAsync();