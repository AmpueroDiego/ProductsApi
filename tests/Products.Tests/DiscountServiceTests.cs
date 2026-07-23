using FluentAssertions;
using Products.Infrastructure.External;

namespace Products.Tests;

public class DiscountServiceTests
{
    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("Simulated network failure.");
        }
    }

    [Fact]
    public async Task GetDiscountAsync_WhenHttpClientThrows_ReturnsZero()
    {
        var httpClient = new HttpClient(new ThrowingHandler())
        {
            BaseAddress = new Uri("https://example.test/")
        };
        var service = new DiscountService(httpClient);

        var discount = await service.GetDiscountAsync(1);

        discount.Should().Be(0);
    }
}
