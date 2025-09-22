using System.Text.Json;
using Cart.Domain.Events;
using Confluent.Kafka;
using Cart.API.Exceptions;

namespace Cart.API.Services;

public class CartService(IProducer<string, string> producer, ILogger<CartService> logger)
{
    public async Task PublishEventAsync(CartEvent @event)
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = @event.CartId.ToString(),
                Value = JsonSerializer.Serialize(@event)
            };

            await producer.ProduceAsync("cart-events", message);
            logger.LogInformation("Event {EventType} published for cart {CartId}", @event.EventType, @event.CartId);
        }
        catch (ProduceException<string, string> ex)
        {
            logger.LogError(ex,
                "Kafka produce error while : {EventType} for cart {CartId}",
                @event.EventType, @event.CartId);

            throw new KafkaPublishException(
                $"Kafka publish failed for cart {@event.CartId} (event: {@event.EventType})", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unexpected error while publishing {EventType} for cart {CartId}",
                @event.EventType, @event.CartId);

            throw new KafkaPublishException(
                $"Unexpected error publishing event {@event.EventType} for cart {@event.CartId}", ex);
        }
    }

    public async Task<IEnumerable<CartEvent>> GetEventsAsync(Guid cartId)
    {
        logger.LogInformation("Retrieving events for cart {CartId}", cartId);
        return await Task.FromResult(Enumerable.Empty<CartEvent>());
    }
}