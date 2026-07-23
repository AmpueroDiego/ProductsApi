using System.Text.Json.Serialization;

namespace Products.Infrastructure.External;

public class DiscountApiResponse
{
    [JsonPropertyName("productId")]
    public int ProductId { get; set; }

    [JsonPropertyName("discount")]
    public int Discount { get; set; }
}
