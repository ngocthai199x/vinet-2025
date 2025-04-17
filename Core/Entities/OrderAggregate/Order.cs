using System;
using Core.Interfaces;

namespace Core.Entities.OrderAggregate;

public class Order : BaseEntity, IDtoConvertible
{
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public required string BuyerEmail { get; set; }
    public ShippingAddress ShippingAddress { get; set; } = null!;
    public DeliveryMethod DeliveryMethod { get; set; } = null!;
    public PaymentSummary PaymentSummary { get; set; } = null!;
    // Use IReadOnlyList here will return error: Collection was of a fixed size
    //public IReadOnlyList<OrderItem> OrderItems { get; set; } = [];
    // List<OrderItem> should be used if you want to return them based on eager loading the entities
    public List<OrderItem> OrderItems { get; set; } = [];
    public decimal Subtotal { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public required string PaymentIntentId { get; set; }
    public decimal GetTotal()
    {
        return Subtotal + DeliveryMethod.Price;
    }
}
