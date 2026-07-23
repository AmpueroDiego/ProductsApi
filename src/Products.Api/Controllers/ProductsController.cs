using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Products.Application.Dtos;
using Products.Application.Services;

namespace Products.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;
    private readonly IValidator<CreateProductRequest> _createValidator;
    private readonly IValidator<UpdateProductRequest> _updateValidator;

    public ProductsController(
        ProductService productService,
        IValidator<CreateProductRequest> createValidator,
        IValidator<UpdateProductRequest> updateValidator)
    {
        _productService = productService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var validation = await _createValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return BuildValidationProblem(validation);
        }

        var productId = await _productService.CreateAsync(request);
        var created = await _productService.GetByIdAsync(productId);
        return CreatedAtAction(nameof(GetById), new { productId }, created);
    }

    [HttpPut("{productId:int}")]
    public async Task<IActionResult> Update(int productId, [FromBody] UpdateProductRequest request)
    {
        var validation = await _updateValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            return BuildValidationProblem(validation);
        }

        var updated = await _productService.UpdateAsync(productId, request);
        return Ok(updated);
    }

    [HttpGet("{productId:int}")]
    public async Task<IActionResult> GetById(int productId)
    {
        var product = await _productService.GetByIdAsync(productId);
        return Ok(product);
    }

    private BadRequestObjectResult BuildValidationProblem(FluentValidation.Results.ValidationResult validation)
    {
        foreach (var error in validation.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        return BadRequest(new ValidationProblemDetails(ModelState));
    }
}
