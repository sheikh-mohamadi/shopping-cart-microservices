using Microsoft.Extensions.DependencyInjection;
using Shared.Kernel.Kafka;

public static class KafkaServiceProviderExtensions
{
    public static async Task EnsureKafkaTopicsAsync(this IServiceProvider services, IEnumerable<string> topics)
    {
        using var scope = services.CreateScope();
        var topicManager = scope.ServiceProvider.GetRequiredService<ITopicManager>();
        await KafkaRetryHelper.EnsureTopicsWithRetryAsync(topicManager, topics);
    }
}