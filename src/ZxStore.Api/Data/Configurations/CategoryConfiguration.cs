using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZxStore.Api.Entities;

namespace ZxStore.Api.Data.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
  public void Configure(EntityTypeBuilder<Category> builder)
  {
    builder.HasData(
        new Category
        {
          Id = 1,
          Name = "Default"
        },
        new Category
        {
          Id = 2,
          Name = "CPU"
        }
        );
  }
}
