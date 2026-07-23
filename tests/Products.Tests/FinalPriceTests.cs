using FluentAssertions;
using Products.Application.Services;

namespace Products.Tests;

public class FinalPriceTests
{
    [Fact]
    public void CalculateFinalPrice_WithNormalDiscount_AppliesPercentage()
    {
        var result = PriceCalculator.CalculateFinalPrice(price: 100m, discount: 20);

        result.Should().Be(80.00m);
    }

    [Fact]
    public void CalculateFinalPrice_WithZeroDiscount_ReturnsSamePrice()
    {
        var result = PriceCalculator.CalculateFinalPrice(price: 100m, discount: 0);

        result.Should().Be(100.00m);
    }

    [Fact]
    public void CalculateFinalPrice_WithFullDiscount_ReturnsZero()
    {
        var result = PriceCalculator.CalculateFinalPrice(price: 100m, discount: 100);

        result.Should().Be(0.00m);
    }

    [Fact]
    public void CalculateFinalPrice_WithDecimalPrice_RoundsToTwoDecimals()
    {
        var result = PriceCalculator.CalculateFinalPrice(price: 99.99m, discount: 10);

        result.Should().Be(89.99m);
    }
}
