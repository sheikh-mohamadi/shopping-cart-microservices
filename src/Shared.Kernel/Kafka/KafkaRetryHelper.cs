using Polly;
using Serilog;

namespace Shared.Kernel.Kafka;

public static class KafkaRetryHelper
{
    public static async Task EnsureTopicsWithRetryAsync(
        ITopicManager topicManager,
        IEnumerable<string> topics,
        int numPartitions = 1,
        short replicationFactor = 1)
    {
        var retryPolicy = Policy
            .Handle<Confluent.Kafka.KafkaException>()
            .Or<Exception>()
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timespan, attempt, context) =>
                {
                    Log.Warning("⚠️ Kafka not ready (attempt {Attempt}). Retrying in {Delay}s. Error: {Error}",
                        attempt, timespan.TotalSeconds, exception.Message);
                });

        await retryPolicy.ExecuteAsync(async () =>
        {
            await topicManager.EnsureTopicsExistAsync(topics, numPartitions, replicationFactor);
            Log.Information("✅ Kafka topics ensured successfully.");
        });
    }
}