using System.Text.Json;
using Confluent.Kafka;
using Cart.Domain.Events;
using StackExchange.Redis;

namespace Cart.Denormalizer;

public class Worker(IServiceProvider serviceProvider, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Cart Denormalizer Service starting...");

        using var scope = serviceProvider.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IConsumer<string, string>>();
        var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>();

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

                    logger.LogDebug("Received message for denormalization: {Message}", consumeResult.Message.Value);

                    var cartEvent = JsonSerializer.Deserialize<CartEvent>(
                        consumeResult.Message.Value,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (cartEvent != null)
                    {
                        var db = redis.GetDatabase();

                        switch (cartEvent)
                        {
                            case ItemAddedEvent itemAdded:
                                await AddItemToCart(db, itemAdded);
                                break;
                            case ItemRemovedEvent itemRemoved:
                                await RemoveItemFromCart(db, itemRemoved);
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

    private async Task AddItemToCart(IDatabase db, ItemAddedEvent @event)
    {
        var cartKey = $"cart:{@event.CartId}";
        var itemKey = $"item:{@event.Item.ProductId}";

        var itemJson = JsonSerializer.Serialize(@event.Item);

        await db.HashSetAsync(cartKey, itemKey, itemJson);
        await db.KeyExpireAsync(cartKey, TimeSpan.FromDays(7));

        logger.LogInformation(
            "📦 Item {ProductId} added to cart {CartId} in read model",
            @event.Item.ProductId, @event.CartId);
    }

    private async Task RemoveItemFromCart(IDatabase db, ItemRemovedEvent @event)
    {
        var cartKey = $"cart:{@event.CartId}";
        var itemKey = $"item:{@event.ProductId}";

        await db.HashDeleteAsync(cartKey, itemKey);

        logger.LogInformation(
            "🗑️ Item {ProductId} removed from cart {CartId} in read model",
            @event.ProductId, @event.CartId);
    }
}