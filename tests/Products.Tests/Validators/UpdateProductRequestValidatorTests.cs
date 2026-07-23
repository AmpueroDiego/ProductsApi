using FluentAssertions;
using Products.Application.Dtos;
using Products.Application.Validators;

namespace Products.Tests.Validators;

public class UpdateProductRequestValidatorTests
{
    private readonly UpdateProductRequestValidator _validator = new();

    private static UpdateProductRequest ValidRequest() => new()
    {
        Name = "Keyboard",
        Status = 0,
        Stock = 0,
        Description = null,
        Price = 10m
    };

    [Fact]
    public void ValidRequest_Passes()
    {
        var result = _validator.Validate(ValidRequest());

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void EmptyName_Fails()
    {
        var request = ValidRequest();
        request.Name = "";

        var result = _validator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProductRequest.Name));
    }
}
