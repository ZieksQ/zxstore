using Microsoft.EntityFrameworkCore;
using ZxStore.Api.Data;
using ZxStore.Api.DTOs.Product;
using ZxStore.Api.Entities;
using ZxStore.Api.Interfaces;

namespace ZxStore.Api.Services;

public class ProductService : IProductService
{
  private readonly AppDbContext _context;
  private readonly ILogger<ProductService> _logger;
  private readonly ICategoryService _categoryService;

  public ProductService(
      AppDbContext context,
      ILogger<ProductService> logger,
      ICategoryService categoryService
      )
  {
    _context = context;
    _logger = logger;
    _categoryService = categoryService;
  }

  public async Task<IEnumerable<Product>> GetProductsAsync()
  {
    return await _context.Products
      .AsNoTracking()
      .Include(p => p.Category)
      .OrderBy(p => p.Name)
      .ToListAsync();
  }

  public async Task<IEnumerable<Product?>> GetProductsByCategoryAsync(int categoryId)
  {
    return await _context.Products
      .AsNoTracking()
      .Where(p => p.CategoryId == categoryId)
      .Include(p => p.Category)
      .OrderBy(p => p.Name)
      .ToListAsync();
  }

  public async Task<IEnumerable<Product?>> SearchProductsAsync(string searchTerm)
  {
    return await _context.Products
      .AsNoTracking()
      .Where(p => p.Name.Contains(searchTerm) ||
          (p.Description != null && p.Description.Contains(searchTerm)))
      .Include(p => p.Category)
      .OrderBy(p => p.Name)
      .ToListAsync();
  }

  public async Task<Product?> GetProductByIdAsync(Guid id)
  {
    return await _context.Products
     .AsNoTracking()
     .Include(p => p.Category)
     .FirstOrDefaultAsync(p => p.Id == id);
  }

  public async Task<Product> CreateProductAsync(ProductDto productDto)
  {
    if (!await _categoryService.CategoryExistsAsync(productDto.CategoryId))
      throw new ArgumentException("Category does not exists");

    Product product = new()
    {
      Name = productDto.Name,
      Description = productDto.Description,
      Price = productDto.Price,
      Stock = productDto.Stock,
      CategoryId = productDto.CategoryId
    };

    _context.Products.Add(product);
    await _context.SaveChangesAsync();
    _logger.LogInformation("Product created");

    return product;
  }

  public async Task<Product?> UpdateProductAsync(Guid id, ProductDto productDto)
  {
    var product = await _context.Products.FindAsync(id);

    if (product is null) return null;

    if (!await _categoryService.CategoryExistsAsync(productDto.CategoryId))
      throw new ArgumentException("Category does not exists");

    product.Name = productDto.Name;
    product.Description = productDto.Description;
    product.Price = productDto.Price;
    product.Stock = productDto.Stock;
    product.CategoryId = productDto.CategoryId;
    product.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    return product;
  }

  public async Task<bool> DeleteProductAsync(Guid id)
  {
    var product = await _context.Products
      .Include(p => p.CartItems)
      .Include(p => p.OrderItems)
      .FirstOrDefaultAsync(p => p.Id == id);

    if (product is null) return false;

    // NOTE: Only use this if you have archived not totally delete the data
    // product.IsDeleted = true;
    // product.DeletedAt = DateTime.UtcNow;

    _context.CartItems.RemoveRange(product.CartItems);
    _context.Products.Remove(product);

    await _context.SaveChangesAsync();
    return true;
  }

  public async Task<bool> UpdateProductStockAsync(Guid id, int quantity)
  {
    var rowsAffected = await _context.Products
      .Where(p => p.Id == id)
      .ExecuteUpdateAsync(setters => setters
          .SetProperty(p => p.Stock, p => p.Stock + quantity)
          .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));

    return rowsAffected > 0;
  }

  public async Task<bool> ProductExistsAsync(Guid id)
  {
    return await _context.Products.AnyAsync(p => p.Id == id);
  }
}
