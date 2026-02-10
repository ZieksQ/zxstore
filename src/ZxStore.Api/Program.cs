using ZxStore.Api.Data;
using ZxStore.Api.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.AddAppDb();

// Services
builder.Services.AddScoped<ICategoryService, ICategoryService>();

var app = builder.Build();

app.MigrateDb();

app.Run();
