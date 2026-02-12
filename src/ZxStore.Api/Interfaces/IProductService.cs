using ZxStore.Api.DTOs.Product;
using ZxStore.Api.Entities;

namespace ZxStore.Api.Interfaces;

public interface IProductService
{
  // TODO: Must have Task<>
  // - Get All Product w/ query params
  // - Get Product by Id
  // - Create new product
  // - Update Product (this may become complex in the future but snapshots of prices can save us)
  // - Delete Product by Id
  // - Boolean tasks -> Delete, Exists
  // - Extras: - Update Price / Discount, Out of Stock

  Task<IEnumerable<Product>> GetProductsAsync();
  Task<IEnumerable<Product?>> GetProductsByCategoryAsync(int categoryId);
  Task<IEnumerable<Product?>> SearchProductsAsync(string searchTerm);
  Task<Product?> GetProductByIdAsync(Guid id);
  Task<Product> CreateProductAsync(ProductDto productDto);
  Task<Product?> UpdateProductAsync(Guid id, ProductDto productDto);
  Task<bool> DeleteProductAsync(Guid id);
  Task<bool> UpdateProductStockAsync(Guid id, int quantity);
  Task<bool> ProductExistsAsync(Guid id);
}
