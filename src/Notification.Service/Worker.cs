using System.Text.Json;
using Confluent.Kafka;

namespace Notification.Service;

public class Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Notification Service starting...");

        using var scope = serviceProvider.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IConsumer<string, string>>();

        try
        {
            consumer.Subscribe("shopping-cart.public.UserProfile");
            logger.LogInformation("Subscribed to user-profile-changes topic");

            while (!stoppingToken.IsCancellationRequested)
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    if (consumeResult?.Message?.Value == null) continue;

                    logger.LogDebug("Received CDC message: {Message}", consumeResult.Message.Value);

                    var changeEvent = JsonSerializer.Deserialize<JsonElement>(consumeResult.Message.Value);

                    if (changeEvent.TryGetProperty("op", out var opElement) &&
                        opElement.GetString() == "u")
                    {
                        var after = changeEvent.GetProperty("after");
                        var userId = after.GetProperty("UserId").GetString();
                        var email = after.GetProperty("Email").GetString();

                        if (!string.IsNullOrWhiteSpace(email))
                        {
                            logger.LogInformation(
                                "📧 Sending notification to user {UserId} with email {Email} about profile update",
                                userId ?? "unknown", email);

                            await SendNotification(email);
                        }
                        else
                        {
                            logger.LogWarning("Skipping notification because email is missing for user {UserId}", userId);
                        }
                    }

                    consumer.Commit(consumeResult);
                }
                catch (ConsumeException ex)
                {
                    logger.LogError(ex, "Error consuming message: {Error}", ex.Error.Reason);
                    await Task.Delay(1000, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing message");
                    await Task.Delay(1000, stoppingToken);
                }
        }
        finally
        {
            consumer.Close();
        }
    }
    
    private async Task SendNotification(string email)
    {
        logger.LogInformation("✅ Notification sent to {Email}", email);
        await Task.Delay(100);
    }
}