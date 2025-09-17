using System.Text.Json;
using Cart.Domain.Events;
using Confluent.Kafka;

namespace Fraud.Service;

public class Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Fraud Service starting...");

        using var scope = serviceProvider.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IConsumer<string, string>>();

        try
        {
            consumer.Subscribe("cart-events");
            logger.LogInformation("Subscribed to cart-events topic");

            while (!stoppingToken.IsCancellationRequested)
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    if (consumeResult?.Message?.Value == null) continue;

                    logger.LogDebug("Received message for fraud check: {Message}", consumeResult.Message.Value);

                    var cartEvent = JsonSerializer.Deserialize<CartEvent>(
                        consumeResult.Message.Value,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (cartEvent != null) await CheckForFraud(cartEvent);

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

    private async Task CheckForFraud(CartEvent @event)
    {
        logger.LogInformation(
            "🔍 Checking for fraud in cart {CartId} for user {UserId}",
            @event.CartId, @event.UserId);

        // پیاده‌سازی منطق تشخیص تقلب
        await Task.Delay(150);

        logger.LogInformation(
            "✅ Fraud check completed for cart {CartId}",
            @event.CartId);
    }
}