namespace Products.Application.Exceptions;

public class ProductAlreadyExistsException : Exception
{
    public ProductAlreadyExistsException(int productId)
        : base($"Product {productId} already exists.")
    {
    }
}
