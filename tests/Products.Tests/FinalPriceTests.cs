using FluentAssertions;
using Products.Application.Services;

namespace Products.Tests;

public class FinalPriceTests
{
    [Theory]
    [InlineData(100, 20, 80.00)]    // descuento normal
    [InlineData(100, 0, 100.00)]    // sin descuento
    [InlineData(100, 100, 0.00)]    // descuento total
    [InlineData(99.99, 10, 89.99)]  // redondeo a 2 decimales
    public void CalculateFinalPrice_ReturnsExpected(decimal price, int discount, decimal expected)
    {
        var result = PriceCalculator.CalculateFinalPrice(price, discount);

        result.Should().Be(expected);
    }
}
