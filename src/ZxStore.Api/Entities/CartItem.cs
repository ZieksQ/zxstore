namespace ZxStore.Api.Entities;

public class CartItem
{
  public Guid Id { get; set; }

  public Guid CartId { get; set; }
  public Cart Cart { get; set; } = null!;

  public Guid ProductId { get; set; }
  public Product Product { get; set; } = null!;

  // You can use this to set the quantity of 
  // products the user wanted to buy then 
  // calculate the total price
  public int Quantity { get; set; }
}
