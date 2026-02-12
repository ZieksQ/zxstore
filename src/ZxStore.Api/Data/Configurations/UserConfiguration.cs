using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZxStore.Api.Entities;

namespace ZxStore.Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    // User Indexer
    builder.HasIndex(u => u.Username)
    .IsUnique();

    builder.HasIndex(u => u.Email)
    .IsUnique();
  }
}
