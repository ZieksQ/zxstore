namespace ZxStore.Api.Entities;

public class User
{
  public Guid Id { get; set; }
  public string Username { get; set; } = string.Empty;
  public string PasswordHash { get; set; } = null!;
  public string Email { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }

  // Foreign Key
  public ICollection<UserRole> UserRoles { get; set; } = [];

  // Navigation Properties
  public Cart Cart { get; set; } = null!;
  public ICollection<Order> Orders { get; set; } = [];
}
