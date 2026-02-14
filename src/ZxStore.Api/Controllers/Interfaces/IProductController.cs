using Microsoft.AspNetCore.Mvc;
using ZxStore.Api.DTOs.Product;
using ZxStore.Api.Entities;

namespace ZxStore.Api.Controllers.Interfaces;

// FIX: Make the ActionResult value DTO

public interface IProductController
{
  Task<ActionResult<IEnumerable<Product>>> GetAll();
  Task<ActionResult<Product>> GetById(Guid id);
  Task<ActionResult<Product>> Create([FromBody] ProductDto dto);
  Task<ActionResult<Product>> Update(Guid id, [FromBody] ProductDto dto);
  Task<ActionResult<bool>> Delete(Guid id);
}
