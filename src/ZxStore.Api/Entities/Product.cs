namespace ZxStore.Api.Entities;

public class Product
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public decimal Price { get; set; }

  private int _stock;
  public int Stock
  {
    get => _stock;
    set
    {
      _stock = value < 0 ? 0 : value; // Prevents negative value but will change later
      // Automatically makes IsAvailable false
      // when Stock hits 0
      if (_stock <= 0) IsAvailable = false;
    }
  }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? ReleaseDate { get; set; }
  public bool IsAvailable { get; set; } = true;

  public bool HasStack => Stock > 0;

  public ICollection<CartItem> CartItems { get; set; } = [];
}
