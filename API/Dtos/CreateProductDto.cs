using System.ComponentModel.DataAnnotations;

namespace API.Dtos;

public record CreateProductDto(
    [Required] string Name,
    [Required] string Description,
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")] decimal Price,
    [Required] string PictureUrl,
    [Required] string Type,
    [Required] string Brand,
    [Range(1, int.MaxValue, ErrorMessage = "Quantity in stock must be at least 1")] int QuantityInStock
);