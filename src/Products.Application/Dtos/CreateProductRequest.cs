using System.ComponentModel.DataAnnotations;

namespace Products.Application.Dtos;

public class CreateProductRequest
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 1, ErrorMessage = "Status must be 0 (Inactive) or 1 (Active).")]
    public int Status { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock must be zero or greater.")]
    public int Stock { get; set; }

    [MaxLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; set; }
}
