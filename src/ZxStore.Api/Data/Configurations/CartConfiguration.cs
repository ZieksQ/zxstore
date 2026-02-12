using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZxStore.Api.Entities;

namespace ZxStore.Api.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
  public void Configure(EntityTypeBuilder<Cart> builder)
  {
    builder.HasOne(c => c.User)
    .WithOne(u => u.Cart)
    .HasForeignKey<Cart>(c => c.UserId)
    .OnDelete(DeleteBehavior.Cascade);
  }
}
