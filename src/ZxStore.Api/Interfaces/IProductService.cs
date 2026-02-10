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
  Task<Product?> GetProductByIdAsync();
  Task<Product> CreateProductAsync();
  Task<Product?> UpdateProductAsync();
  Task<bool> DeleteProductAsync();
  Task<bool> ProductExistsAsync();
}
