using Notification.Service;
using Shared.Kernel;
using Shared.Kernel.Kafka;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddKafkaServices(context.Configuration);
        services.AddKafkaConsumer(context.Configuration, "notification-service-group");
        services.AddHostedService<Worker>();
    })
    .Build();

using var scope = host.Services.CreateScope();
var topicManager = scope.ServiceProvider.GetRequiredService<ITopicManager>();
await topicManager.EnsureTopicsExistAsync(["shopping-cart.public.UserProfile"]);

await host.RunAsync();