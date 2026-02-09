using Microsoft.EntityFrameworkCore;
using ZxStore.Api.Entities;

namespace ZxStore.Api.Data;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
  {

  }

  public DbSet<User> Users => Set<User>();
  public DbSet<Role> Roles => Set<Role>();
  public DbSet<UserRole> UserRoles => Set<UserRole>();
  public DbSet<Product> Products => Set<Product>();
  public DbSet<Category> Categories => Set<Category>();
  public DbSet<Cart> Carts => Set<Cart>();
  public DbSet<CartItem> CartItems => Set<CartItem>();
  public DbSet<Order> Orders => Set<Order>();
  public DbSet<OrderItem> OrderItems => Set<OrderItem>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<UserRole>()
      .HasKey(ur => new { ur.UserId, ur.RoleId });

    modelBuilder.Entity<UserRole>()
      .HasOne(ur => ur.User)
      .WithMany(u => u.UserRoles)
      .HasForeignKey(ur => ur.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<UserRole>()
      .HasOne(ur => ur.Role)
      .WithMany(r => r.UserRoles)
      .HasForeignKey(ur => ur.RoleId)
      .OnDelete(DeleteBehavior.Restrict);

    // Cart Item Relationship M:N
    modelBuilder.Entity<CartItem>()
      .HasIndex(ci => new { ci.CartId, ci.ProductId })
      .IsUnique();

    modelBuilder.Entity<CartItem>()
      .HasOne(ci => ci.Cart)
      .WithMany(c => c.CartItems)
      .HasForeignKey(ci => ci.CartId)
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<CartItem>()
      .HasOne(ci => ci.Product)
      .WithMany(p => p.CartItems)
      .HasForeignKey(ci => ci.ProductId)
      .OnDelete(DeleteBehavior.Restrict);

    // Cart Relationship to User
    modelBuilder.Entity<Cart>()
      .HasOne(c => c.User)
      .WithMany(u => u.Carts)
      .HasForeignKey(c => c.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    // Order and User Relationship
    modelBuilder.Entity<Order>()
      .HasOne(o => o.User)
      .WithMany(u => u.Orders)
      .HasForeignKey(o => o.UserId)
      .OnDelete(DeleteBehavior.Restrict);

    // Product Relationship to Category
    modelBuilder.Entity<Product>()
      .HasOne(p => p.Category)
      .WithMany(c => c.Products)
      .HasForeignKey(p => p.CategoryId)
      .OnDelete(DeleteBehavior.Restrict);

    // Order Item Relationship M:N
    modelBuilder.Entity<OrderItem>()
      .HasKey(oi => new { oi.OrderId, oi.ProductId });

    modelBuilder.Entity<OrderItem>()
      .HasOne(oi => oi.Order)
      .WithMany(o => o.OrderItems)
      .HasForeignKey(oi => oi.OrderId)
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<OrderItem>()
      .HasOne(oi => oi.Product)
      .WithMany(p => p.OrderItems)
      .HasForeignKey(oi => oi.ProductId)
      .OnDelete(DeleteBehavior.Cascade);

    // User Indexer
    modelBuilder.Entity<User>()
      .HasIndex(u => u.Username)
      .IsUnique();

    modelBuilder.Entity<User>()
      .HasIndex(u => u.Email)
      .IsUnique();

    // Product Indexer
    modelBuilder.Entity<Product>()
      .HasIndex(p => p.CategoryId);

    modelBuilder.Entity<Product>()
      .HasIndex(p => p.CreatedAt);

    modelBuilder.Entity<Product>()
      .HasIndex(p => p.Price);


    // Order Indexer
    modelBuilder.Entity<Order>()
      .HasIndex(o => new { o.UserId, o.OrderDate, o.Status });

    // Decimal Precision
    modelBuilder.Entity<Product>()
      .Property(p => p.Price)
      .HasPrecision(18, 2);

    modelBuilder.Entity<Order>()
      .Property(o => o.TotalAmount)
      .HasPrecision(18, 2);

    modelBuilder.Entity<OrderItem>()
      .Property(oi => oi.UnitPrice)
      .HasPrecision(18, 2);
  }
}
