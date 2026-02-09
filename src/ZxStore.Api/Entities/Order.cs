namespace ZxStore.Api.Entities;

public class Order
{
  public Guid Id { get; set; }

  public Guid UserId { get; set; }
  public User? User { get; set; } = null;

  public DateTime OrderDate { get; set; } = DateTime.UtcNow;
  public OrderStatus Status { get; set; } = OrderStatus.Pending;
  public decimal TotalAmount { get; set; } // added total amount since price can change overtime
  public string ShippingAddress { get; set; } = string.Empty;
  public string? TrackingNumber { get; set; }
  public DateTime? ShippedDate { get; set; }
  public DateTime? DeliveredDate { get; set; }

  public ICollection<OrderItem> OrderItems { get; set; } = [];
}
