using Microsoft.EntityFrameworkCore;

namespace ZxStore.Api.Data;

public static class DataExtensions
{
  public static void MigrateDb(this WebApplication app)
  {
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider
      .GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
  }

  public static void AddAppDb(this WebApplicationBuilder builder)
  {
    var connString = builder.Configuration.GetConnectionString("ecommerce_development")
      ?? throw new InvalidOperationException($"Connection String does not exists.");
    builder.Services.AddSqlite<AppDbContext>(connString);
  }
}
