using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;

namespace Shared.Kernel.Kafka;

public interface ITopicManager
{
    Task EnsureTopicsExistAsync(IEnumerable<string> topics, int numPartitions = 3, short replicationFactor = 1);
}

public class TopicManager(IAdminClient adminClient, ILogger<TopicManager> logger) : ITopicManager
{
    public async Task EnsureTopicsExistAsync(IEnumerable<string> topics, int numPartitions = 3,
        short replicationFactor = 1)
    {
        try
        {
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            var existingTopics = metadata.Topics.Select(t => t.Topic).ToList();

            var topicsToCreate = topics.Where(topic => !existingTopics.Contains(topic)).ToList();

            if (!topicsToCreate.Any())
            {
                logger.LogInformation("All required topics already exist");
                return;
            }

            var topicSpecifications = topicsToCreate.Select(topic =>
                new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = numPartitions,
                    ReplicationFactor = replicationFactor
                }).ToList();

            logger.LogInformation("Creating topics: {Topics}", string.Join(", ", topicsToCreate));

            await adminClient.CreateTopicsAsync(topicSpecifications);

            logger.LogInformation("Successfully created topics: {Topics}", string.Join(", ", topicsToCreate));
        }
        catch (CreateTopicsException ex)
        {
            logger.LogError(ex, "Error creating topics: {Reason}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ensuring topics exist");
            throw;
        }
    }
}