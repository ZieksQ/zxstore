namespace ZxStore.Api.Entities;

public class Product : BaseEntity
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public decimal Price { get; set; }
  public int Stock { get; set; }

  public int CategoryId { get; set; }
  public Category Category { get; set; } = null!;

  public ICollection<CartItem> CartItems { get; set; } = [];
  public ICollection<OrderItem> OrderItems { get; set; } = [];
}
