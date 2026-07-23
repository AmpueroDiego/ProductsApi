using FluentAssertions;
using NSubstitute;
using Products.Application.Dtos;
using Products.Application.Exceptions;
using Products.Application.Interfaces;
using Products.Application.Services;
using Products.Domain;

namespace Products.Tests;

public class ProductServiceTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly IDiscountService _discountService = Substitute.For<IDiscountService>();
    private readonly IStatusCache _statusCache = Substitute.For<IStatusCache>();
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _service = new ProductService(_repository, _discountService, _statusCache);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsComposedResponse()
    {
        var product = new Product
        {
            ProductId = 1,
            Name = "Keyboard",
            Status = 1,
            Stock = 10,
            Description = "Mechanical",
            Price = 100m
        };
        _repository.GetByIdAsync(1).Returns(product);
        _discountService.GetDiscountAsync(1).Returns(20);
        _statusCache.GetStatusName(1).Returns("Active");

        var result = await _service.GetByIdAsync(1);

        result.ProductId.Should().Be(1);
        result.Name.Should().Be("Keyboard");
        result.StatusName.Should().Be("Active");
        result.Stock.Should().Be(10);
        result.Description.Should().Be("Mechanical");
        result.Price.Should().Be(100m);
        result.Discount.Should().Be(20);
        result.FinalPrice.Should().Be(80m);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ThrowsProductNotFound()
    {
        _repository.GetByIdAsync(99).Returns((Product?)null);

        var act = () => _service.GetByIdAsync(99);

        await act.Should().ThrowAsync<ProductNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_WhenProductIsNew_PersistsAndReturnsId()
    {
        var request = new CreateProductRequest
        {
            ProductId = 5,
            Name = "Mouse",
            Status = 1,
            Stock = 3,
            Price = 25m
        };
        _repository.ExistsAsync(5).Returns(false);

        var id = await _service.CreateAsync(request);

        id.Should().Be(5);
        await _repository.Received(1).CreateAsync(Arg.Is<Product>(p => p != null && p.ProductId == 5 && p.Name == "Mouse"));
    }

    [Fact]
    public async Task CreateAsync_WhenProductAlreadyExists_ThrowsConflict()
    {
        var request = new CreateProductRequest { ProductId = 5, Name = "Mouse", Status = 1, Stock = 3, Price = 25m };
        _repository.ExistsAsync(5).Returns(true);

        var act = () => _service.CreateAsync(request);

        await act.Should().ThrowAsync<ProductAlreadyExistsException>();
        await _repository.DidNotReceive().CreateAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task UpdateAsync_WhenProductExists_PersistsAndReturnsResponse()
    {
        var request = new UpdateProductRequest
        {
            Name = "Updated",
            Status = 0,
            Stock = 7,
            Price = 50m
        };
        _repository.ExistsAsync(1).Returns(true);
        _discountService.GetDiscountAsync(1).Returns(0);
        _statusCache.GetStatusName(0).Returns("Inactive");

        var result = await _service.UpdateAsync(1, request);

        await _repository.Received(1).UpdateAsync(Arg.Is<Product>(p => p != null && p.ProductId == 1 && p.Name == "Updated"));
        result.Name.Should().Be("Updated");
        result.StatusName.Should().Be("Inactive");
        result.FinalPrice.Should().Be(50m);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductDoesNotExist_ThrowsProductNotFound()
    {
        var request = new UpdateProductRequest { Name = "Updated", Status = 0, Stock = 7, Price = 50m };
        _repository.ExistsAsync(1).Returns(false);

        var act = () => _service.UpdateAsync(1, request);

        await act.Should().ThrowAsync<ProductNotFoundException>();
    }
}
