namespace Products.Application.Exceptions;

public class ProductNotFoundException : Exception
{
    public ProductNotFoundException(int productId)
        : base($"Product {productId} was not found.")
    {
    }
}
