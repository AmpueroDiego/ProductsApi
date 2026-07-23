namespace Products.Application.Dtos;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public int Status { get; set; }
    public int Stock { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
}
