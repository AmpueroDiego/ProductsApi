using Microsoft.AspNetCore.Mvc;
using Products.Application.Dtos;
using Products.Application.Services;

namespace Products.Api.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    /// <summary>Crea un producto nuevo.</summary>
    /// <param name="request">Datos del producto a crear.</param>
    /// <response code="201">Producto creado correctamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    /// <response code="409">Ya existe un producto con ese Id.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var productId = await _productService.CreateAsync(request);
        var created = await _productService.GetByIdAsync(productId);
        return CreatedAtAction(nameof(GetById), new { productId }, created);
    }

    /// <summary>Actualiza un producto existente.</summary>
    /// <param name="productId">Id del producto a actualizar.</param>
    /// <param name="request">Nuevos datos del producto.</param>
    /// <response code="200">Producto actualizado correctamente.</response>
    /// <response code="400">Los datos enviados no son válidos.</response>
    /// <response code="404">No existe un producto con ese Id.</response>
    [HttpPut("{productId:int}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int productId, [FromBody] UpdateProductRequest request)
    {
        var updated = await _productService.UpdateAsync(productId, request);
        return Ok(updated);
    }

    /// <summary>Obtiene un producto por su Id.</summary>
    /// <param name="productId">Id del producto a consultar.</param>
    /// <response code="200">Producto encontrado.</response>
    /// <response code="404">No existe un producto con ese Id.</response>
    [HttpGet("{productId:int}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int productId)
    {
        var product = await _productService.GetByIdAsync(productId);
        return Ok(product);
    }
}
