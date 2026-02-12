using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZxStore.Api.Entities;

namespace ZxStore.Api.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
  public void Configure(EntityTypeBuilder<Order> builder)
  {
    builder.HasOne(o => o.User)
    .WithMany(u => u.Orders)
    .HasForeignKey(o => o.UserId)
    .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(o => new { o.UserId, o.OrderDate, o.Status });

    builder.Property(o => o.TotalAmount)
    .HasPrecision(18, 2);
  }
}
