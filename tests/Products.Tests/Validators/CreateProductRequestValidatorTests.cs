using FluentAssertions;
using FluentValidation.Results;
using Products.Application.Dtos;
using Products.Application.Validators;

namespace Products.Tests.Validators;

public class CreateProductRequestValidatorTests
{
    private readonly CreateProductRequestValidator _validator = new();

    private static CreateProductRequest ValidRequest() => new()
    {
        ProductId = 1,
        Name = "Keyboard",
        Status = 1,
        Stock = 10,
        Description = "A mechanical keyboard",
        Price = 99.90m
    };

    [Fact]
    public void ValidRequest_Passes()
    {
        ValidationResult result = _validator.Validate(ValidRequest());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void EmptyName_Fails()
    {
        var request = ValidRequest();
        request.Name = "";

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.Name));
    }

    [Fact]
    public void NameLongerThan100_Fails()
    {
        var request = ValidRequest();
        request.Name = new string('a', 101);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.Name));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void StatusOtherThanZeroOrOne_Fails(int status)
    {
        var request = ValidRequest();
        request.Status = status;

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.Status));
    }

    [Fact]
    public void NegativeStock_Fails()
    {
        var request = ValidRequest();
        request.Stock = -1;

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.Stock));
    }

    [Fact]
    public void PriceZeroOrLess_Fails()
    {
        var request = ValidRequest();
        request.Price = 0m;

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.Price));
    }

    [Fact]
    public void DescriptionLongerThan500_Fails()
    {
        var request = ValidRequest();
        request.Description = new string('a', 501);

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductRequest.Description));
    }
}
