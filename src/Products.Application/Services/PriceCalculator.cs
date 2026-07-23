namespace Products.Application.Services;

public static class PriceCalculator
{
    public static decimal CalculateFinalPrice(decimal price, int discount)
    {
        var finalPrice = price * (100 - discount) / 100m;
        return Math.Round(finalPrice, 2);
    }
}
