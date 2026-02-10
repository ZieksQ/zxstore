using ZxStore.Api.Entities;

namespace ZxStore.Api.Interfaces;

public interface ICategoryService
{
  Task<IEnumerable<Category>> GetAllCategoryAsync();
  Task<Category?> GetCategoryByIdAsync(int id);
  // NOTE: Might add DTOs for create and update string name for validation
  Task<Category> CreateCategoryAsync(string name);
  Task<Category?> UpdateCategoryAsync(int id, string name);
  Task<bool> DeleteCategoryAsync(int id);
  Task<bool> CategoryExistsAsync(int id);
}
