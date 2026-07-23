using Dapper;
using Products.Application.Interfaces;
using Products.Domain;
using Products.Infrastructure.Data;

namespace Products.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Product?> GetByIdAsync(int productId)
    {
        const string sql = @"
            SELECT ProductId, Name, Status, Stock, Description, Price
            FROM dbo.Products
            WHERE ProductId = @ProductId;";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<Product>(sql, new { ProductId = productId });
    }

    public async Task CreateAsync(Product product)
    {
        const string sql = @"
            INSERT INTO dbo.Products (ProductId, Name, Status, Stock, Description, Price)
            VALUES (@ProductId, @Name, @Status, @Stock, @Description, @Price);";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, product);
    }

    public async Task UpdateAsync(Product product)
    {
        const string sql = @"
            UPDATE dbo.Products
            SET Name = @Name,
                Status = @Status,
                Stock = @Stock,
                Description = @Description,
                Price = @Price
            WHERE ProductId = @ProductId;";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, product);
    }

    public async Task<bool> ExistsAsync(int productId)
    {
        const string sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM dbo.Products WHERE ProductId = @ProductId
            ) THEN 1 ELSE 0 END;";

        using var connection = _connectionFactory.CreateConnection();
        var exists = await connection.ExecuteScalarAsync<int>(sql, new { ProductId = productId });
        return exists == 1;
    }
}
