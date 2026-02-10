using Microsoft.EntityFrameworkCore;
using ZxStore.Api.Data;
using ZxStore.Api.Entities;
using ZxStore.Api.Interfaces;

namespace ZxStore.Api.Services;

public class CategoryService : ICategoryService
{
  private readonly AppDbContext _context;
  private readonly ILogger<CategoryService> _logger;

  public CategoryService(AppDbContext context, ILogger<CategoryService> logger)
  {
    _context = context;
    _logger = logger;
  }

  public async Task<IEnumerable<Category>> GetAllCategoryAsync()
  {
    _logger.LogInformation("fetching all categories");

    return await _context.Categories
      .AsNoTracking()
      .OrderBy(c => c.Name)
      .ToListAsync();
  }

  public async Task<Category?> GetCategoryByIdAsync(int id)
  {
    _logger.LogInformation("fetching category by id");

    return await _context.Categories
      .AsNoTracking()
      .FirstOrDefaultAsync(c => c.Id == id);
  }

  public async Task<Category> CreateCategoryAsync(string name)
  {
    var category = new Category { Name = name };

    _context.Categories.Add(category);
    await _context.SaveChangesAsync();

    _logger.LogInformation("Category created");
    return category;
  }

  public async Task<Category?> UpdateCategoryAsync(int id, string name)
  {
    var category = await _context.Categories.FindAsync(id);

    if (category is null)
    {
      _logger.LogWarning("Category not found");
      return null;
    }

    category.Name = name;
    await _context.SaveChangesAsync();

    _logger.LogInformation("Category updated");
    return category;
  }

  public async Task<bool> DeleteCategoryAsync(int id)
  {
    var category = await _context.Categories
      .Include(c => c.Products)
      .FirstOrDefaultAsync(c => c.Id == id);

    if (category is null) return false;

    if (category.Products.Any())
      throw new InvalidOperationException("Cannot delete category with products");

    _context.Categories.Remove(category);
    await _context.SaveChangesAsync();

    return true;
  }

  public async Task<bool> CategoryExistsAsync(int id)
  {
    return await _context.Categories.AnyAsync(c => c.Id == id);
  }
}
