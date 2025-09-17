using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Kernel.Kafka;

namespace Shared.Kernel;

public static class ServiceExtensions
{
    public static IServiceCollection AddKafkaServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAdminClient>(sp =>
        {
            var config = new AdminClientConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092"
            };
            return new AdminClientBuilder(config).Build();
        });

        services.AddSingleton<ITopicManager, TopicManager>();

        return services;
    }

    public static IServiceCollection AddKafkaConsumer(this IServiceCollection services, IConfiguration configuration,
        string groupId)
    {
        services.AddSingleton<IConsumer<string, string>>(sp =>
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false
            };

            return new ConsumerBuilder<string, string>(config).Build();
        });

        return services;
    }

    public static IServiceCollection AddKafkaProducer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IProducer<string, string>>(sp =>
        {
            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
                ClientId = "kafka-producer",
                Acks = Acks.All,
                EnableIdempotence = true
            };

            return new ProducerBuilder<string, string>(config).Build();
        });

        return services;
    }
}