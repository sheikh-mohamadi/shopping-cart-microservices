// src/Cart.API/Services/CartService.cs

using System.Text.Json;
using Confluent.Kafka;
using Cart.Domain.Events;

namespace Cart.API.Services;

public class CartService
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<CartService> _logger;

    public CartService(IProducer<string, string> producer, ILogger<CartService> logger)
    {
        _producer = producer;
        _logger = logger;
    }

    public async Task PublishEventAsync(CartEvent @event)
    {
        try
        {
            var message = new Message<string, string>
            {
                Key = @event.CartId.ToString(),
                Value = JsonSerializer.Serialize(@event)
            };

            await _producer.ProduceAsync("cart-events", message);
            _logger.LogInformation("Event {EventType} published for cart {CartId}", @event.EventType, @event.CartId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event for cart {CartId}", @event.CartId);
            throw;
        }
    }

    public async Task<IEnumerable<CartEvent>> GetEventsAsync(Guid cartId)
    {
        // این متد نیاز به پیاده‌سازی دارد
        // باید رویدادها را از event store بازیابی کند
        _logger.LogInformation("Retrieving events for cart {CartId}", cartId);
        return await Task.FromResult(Enumerable.Empty<CartEvent>());
    }
}