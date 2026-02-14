using Microsoft.AspNetCore.Mvc;
using ZxStore.Api.Controllers.Interfaces;
using ZxStore.Api.DTOs.Product;
using ZxStore.Api.Entities;
using ZxStore.Api.Interfaces;

namespace ZxStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase, IProductController
{
  private readonly IProductService _service;
  private readonly ILogger<ProductsController> _logger;

  public ProductsController(
      IProductService service,
      ILogger<ProductsController> logger)
  {
    _service = service;
    _logger = logger;
  }

  [HttpPost]
  public async Task<ActionResult<Product>> Create([FromBody] ProductDto dto)
  {
    var product = await _service.CreateProductAsync(dto);

    return CreatedAtAction(
        nameof(GetById),
        new { id = product.Id },
        product);
  }

  public async Task<ActionResult<bool>> Delete(Guid id)
  {
    throw new NotImplementedException();
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<Product>>> GetAll()
  {
    var products = await _service.GetProductsAsync();

    // WARNING: DTO Mapping should be at the Service layer.
    var results = products.Select(p => new ProductDto(
          p.Name,
          p.Description,
          p.Price,
          p.Stock,
          p.CategoryId
      ));
    return Ok(results);
  }

  [HttpGet("{id:Guid}")]
  public async Task<ActionResult<Product>> GetById(Guid id)
  {
    var product = await _service.GetProductByIdAsync(id);

    if (product is null)
    {
      _logger.LogWarning("Product {id} not found", id);
      return NotFound(new { Message = $"Product with {id} id not found." });
    }

    return Ok(product);
  }

  public async Task<ActionResult<Product>> Update(Guid id, [FromBody] ProductDto dto)
  {
    throw new NotImplementedException();
  }
}
