using Cart.API.Services;
using Cart.Domain.Events;
using Cart.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Cart.API.Controllers;

[ApiController]
[Route("api/[controller]/{cartId:guid}")]
public class CartController(
    CartService cartService,
    ILogger<CartController> logger)
    : ControllerBase
{
    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        Guid cartId,
        [FromBody] AddItemRequest request)
    {
        try
        {
            var @event = new ItemAddedEvent
            {
                CartId = cartId,
                UserId = request.UserId,
                Item = new CartItem
                {
                    ProductId = request.ProductId,
                    ProductName = request.ProductName,
                    Price = request.Price,
                    Quantity = request.Quantity
                }
            };

            await cartService.PublishEventAsync(@event);
            return Ok(new { Message = "Item added successfully", CartId = cartId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding item to cart {CartId}", cartId);
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpDelete("items/{productId:guid}")]
    public async Task<IActionResult> RemoveItem(
        Guid cartId,
        Guid productId,
        [FromBody] RemoveItemRequest request)
    {
        try
        {
            var @event = new ItemRemovedEvent
            {
                CartId = cartId,
                UserId = request.UserId,
                ProductId = productId
            };

            await cartService.PublishEventAsync(@event);
            return Ok(new { Message = "Item removed successfully", CartId = cartId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing item from cart {CartId}", cartId);
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }

    [HttpGet("events")]
    public async Task<IActionResult> GetEvents(Guid cartId)
    {
        try
        {
            var events = await cartService.GetEventsAsync(cartId);
            return Ok(events);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving events for cart {CartId}", cartId);
            return StatusCode(500, new { Error = "Internal server error" });
        }
    }
}

public record AddItemRequest(
    Guid UserId,
    Guid ProductId,
    string ProductName,
    decimal Price,
    int Quantity);

public record RemoveItemRequest(Guid UserId);