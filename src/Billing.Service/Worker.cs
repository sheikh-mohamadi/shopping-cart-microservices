using System.Text.Json;
using Confluent.Kafka;
using Cart.Domain.Events;

namespace Billing.Service;

public class Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Billing Service starting...");

        using var scope = serviceProvider.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IConsumer<string, string>>();

        try
        {
            consumer.Subscribe("cart-events");
            logger.LogInformation("Subscribed to cart-events topic");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    if (consumeResult?.Message?.Value == null)
                    {
                        continue;
                    }

                    logger.LogDebug("Received message: {Message}", consumeResult.Message.Value);

                    var cartEvent = JsonSerializer.Deserialize<CartEvent>(
                        consumeResult.Message.Value,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (cartEvent != null)
                    {
                        switch (cartEvent)
                        {
                            case ItemAddedEvent itemAdded:
                                await ProcessItemAdded(itemAdded);
                                break;
                            case ItemRemovedEvent itemRemoved:
                                await ProcessItemRemoved(itemRemoved);
                                break;
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
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task ProcessItemAdded(ItemAddedEvent @event)
    {
        logger.LogInformation(
            "💰 Processing billing for added item: {ProductName} (ID: {ProductId}) in cart {CartId}",
            @event.Item.ProductName, @event.Item.ProductId, @event.CartId);

        await Task.Delay(200);

        logger.LogInformation(
            "✅ Billing processed for item: {ProductName} in cart {CartId}",
            @event.Item.ProductName, @event.CartId);
    }

    private async Task ProcessItemRemoved(ItemRemovedEvent @event)
    {
        logger.LogInformation(
            "💰 Processing billing for removed item: {ProductId} from cart {CartId}",
            @event.ProductId, @event.CartId);

        await Task.Delay(150);

        logger.LogInformation(
            "✅ Billing processed for removed item: {ProductId} from cart {CartId}",
            @event.ProductId, @event.CartId);
    }
}