namespace ZxStore.Api.Entities;

public class OrderItem
{
  public Guid Id { get; set; }

  public Guid OrderId { get; set; }
  public Order Order { get; set; } = null!;

  public Guid ProductId { get; set; }
  public Product Product { get; set; } = null!;

  public int Quantity { get; set; } = 1;
  public decimal UnitPrice { get; set; }

  public decimal LineTotal => UnitPrice * Quantity; // line total is a real world btw same as the calculation
}
