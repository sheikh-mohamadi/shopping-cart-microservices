namespace Cart.Domain.Models;

public class CartItem
{
    public required Guid ProductId { get; set; }
    public required string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}