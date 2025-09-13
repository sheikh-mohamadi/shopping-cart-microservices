using System.Text.Json.Serialization;
using Cart.Domain.Models;

namespace Cart.Domain.Events;

[JsonDerivedType(typeof(ItemAddedEvent), typeDiscriminator: "item_added")]
[JsonDerivedType(typeof(ItemRemovedEvent), typeDiscriminator: "item_removed")]
public abstract class CartEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public Guid CartId { get; set; }
    public Guid UserId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}

public class ItemAddedEvent : CartEvent
{
    public override string EventType => "item_added";
    public required CartItem Item { get; set; }
}

public class ItemRemovedEvent : CartEvent
{
    public override string EventType => "item_removed";
    public required Guid ProductId { get; set; }
}
