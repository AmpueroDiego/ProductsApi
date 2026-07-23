using Products.Application.Dtos;
using Products.Application.Exceptions;
using Products.Application.Interfaces;
using Products.Domain;

namespace Products.Application.Services;

public class ProductService
{
    private readonly IProductRepository _repository;
    private readonly IDiscountService _discountService;
    private readonly IStatusCache _statusCache;

    public ProductService(
        IProductRepository repository,
        IDiscountService discountService,
        IStatusCache statusCache)
    {
        _repository = repository;
        _discountService = discountService;
        _statusCache = statusCache;
    }

    public async Task<ProductResponse> GetByIdAsync(int productId)
    {
        var product = await _repository.GetByIdAsync(productId);
        if (product is null)
        {
            throw new ProductNotFoundException(productId);
        }

        return await BuildResponseAsync(product);
    }

    public async Task<int> CreateAsync(CreateProductRequest request)
    {
        if (await _repository.ExistsAsync(request.ProductId))
        {
            throw new ProductAlreadyExistsException(request.ProductId);
        }

        var product = new Product
        {
            ProductId = request.ProductId,
            Name = request.Name,
            Status = request.Status,
            Stock = request.Stock,
            Description = request.Description,
            Price = request.Price
        };

        await _repository.CreateAsync(product);
        return product.ProductId;
    }

    public async Task<ProductResponse> UpdateAsync(int productId, UpdateProductRequest request)
    {
        if (!await _repository.ExistsAsync(productId))
        {
            throw new ProductNotFoundException(productId);
        }

        var product = new Product
        {
            ProductId = productId,
            Name = request.Name,
            Status = request.Status,
            Stock = request.Stock,
            Description = request.Description,
            Price = request.Price
        };

        await _repository.UpdateAsync(product);
        return await BuildResponseAsync(product);
    }

    private async Task<ProductResponse> BuildResponseAsync(Product product)
    {
        var discount = await _discountService.GetDiscountAsync(product.ProductId);
        var statusName = _statusCache.GetStatusName(product.Status);

        return new ProductResponse
        {
            ProductId = product.ProductId,
            Name = product.Name,
            StatusName = statusName,
            Stock = product.Stock,
            Description = product.Description,
            Price = product.Price,
            Discount = discount,
            FinalPrice = PriceCalculator.CalculateFinalPrice(product.Price, discount)
        };
    }
}
