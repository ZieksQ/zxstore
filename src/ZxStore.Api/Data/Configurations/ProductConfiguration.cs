using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZxStore.Api.Entities;

namespace ZxStore.Api.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
  public void Configure(EntityTypeBuilder<Product> builder)
  {
    // Product Relationship
    builder.HasOne(p => p.Category)
    .WithMany(c => c.Products)
    .HasForeignKey(p => p.CategoryId)
    .OnDelete(DeleteBehavior.Restrict);

    // Product Indexer
    builder.HasIndex(p => p.CategoryId);

    builder.HasIndex(p => p.CreatedAt);

    builder.HasIndex(p => p.Price);

    // Decimal Precision
    builder.Property(p => p.Price)
    .HasPrecision(18, 2);
  }
}
