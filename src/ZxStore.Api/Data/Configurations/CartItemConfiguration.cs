using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZxStore.Api.Entities;

namespace ZxStore.Api.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
  public void Configure(EntityTypeBuilder<CartItem> builder)
  {
    builder.HasIndex(ci => new { ci.CartId, ci.ProductId })
      .IsUnique();

    builder.HasOne(ci => ci.Cart)
      .WithMany(c => c.CartItems)
      .HasForeignKey(ci => ci.CartId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(ci => ci.Product)
      .WithMany(p => p.CartItems)
      .HasForeignKey(ci => ci.ProductId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
