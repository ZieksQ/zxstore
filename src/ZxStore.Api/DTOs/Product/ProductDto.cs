using System.ComponentModel.DataAnnotations;

namespace ZxStore.Api.DTOs.Product;

public record ProductDto(
    [Required]
    string Name,
    string? Description,
    [Range(0, double.MaxValue)]
    decimal Price,
    [Range(0, int.MaxValue)]
    int Stock,
    int CategoryId
);
