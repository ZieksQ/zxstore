namespace ZxStore.Api.Entities;

public class Cart
{
  public Guid Id { get; set; }

  public Guid UserId { get; set; }
  public User User { get; set; } = null!;

  public ICollection<CartItem> CartItems { get; set; } = [];
}
