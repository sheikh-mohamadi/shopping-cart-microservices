using System.Text.Json;
using Cart.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Cart.API.Controllers;

[ApiController]
[Route("api/cart/view")]
public class CartViewController(IConnectionMultiplexer redis) : ControllerBase
{
    [HttpGet("{cartId:guid}")]
    public async Task<IActionResult> GetCartView(Guid cartId)
    {
        var db = redis.GetDatabase();
        var cartKey = $"cart:{cartId}";

        var items = await db.HashGetAllAsync(cartKey);

        var cartItems = items.Select(item =>
            JsonSerializer.Deserialize<CartItem>(item.Value)).ToList();

        var total = cartItems.Sum(i => i.Price * i.Quantity);

        return Ok(new CartView
        {
            CartId = cartId,
            Items = cartItems,
            Total = total,
            ItemCount = cartItems.Count
        });
    }
}

public record CartView
{
    public Guid CartId { get; set; }
    public List<CartItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
}