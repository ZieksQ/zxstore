using System.ComponentModel.DataAnnotations;

namespace ZxStore.Api.DTOs.Product;

public record ProductDto(
    [Required]
    string Name,

    string? Description,

    [Required]
    [Range(0, double.MaxValue)]
    decimal Price,

    [Required]
    [Range(0, int.MaxValue)]
    int Stock,

    [Range(0, int.MaxValue)]
    int CategoryId
);
