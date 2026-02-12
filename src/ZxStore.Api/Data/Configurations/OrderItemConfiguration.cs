using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZxStore.Api.Entities;

namespace ZxStore.Api.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
  public void Configure(EntityTypeBuilder<OrderItem> builder)
  {

    builder.HasKey(oi => new { oi.OrderId, oi.ProductId });

    builder.HasOne(oi => oi.Order)
      .WithMany(o => o.OrderItems)
      .HasForeignKey(oi => oi.OrderId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(oi => oi.Product)
      .WithMany(p => p.OrderItems)
      .HasForeignKey(oi => oi.ProductId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.Property(oi => oi.UnitPrice)
    .HasPrecision(18, 2);
  }
}
