namespace Products.Application.Dtos;

public class ProductResponse
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public int Stock { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Discount { get; set; }
    public decimal FinalPrice { get; set; }
}
