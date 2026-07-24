using System.Net.Http.Json;
using Products.Application.Interfaces;

namespace Products.Infrastructure.External;

public class DiscountService : IDiscountService
{
    private readonly HttpClient _httpClient;

    public DiscountService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int> GetDiscountAsync(int productId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<DiscountApiResponse>($"discounts/{productId}");
            if (response is null || response.Discount < 0 || response.Discount > 100)
            {
                return 0;
            }

            return response.Discount;
        }
        catch
        {
            return 0;
        }
    }
}
