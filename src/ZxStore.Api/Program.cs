using ZxStore.Api.Data;
using ZxStore.Api.Interfaces;
using ZxStore.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddAppDb();

// Services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

app.MigrateDb();

app.Run();
