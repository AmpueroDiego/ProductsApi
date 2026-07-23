using Products.Domain;

namespace Products.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int productId);
    Task CreateAsync(Product product);
    Task UpdateAsync(Product product);
    Task<bool> ExistsAsync(int productId);
}
