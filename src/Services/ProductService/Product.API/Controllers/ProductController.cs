using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Application.Commands.CreateProduct;
using Product.Application.Commands.DeleteProduct;
using Product.Application.Commands.UpdateProduct;
using Product.Application.DTOs;
using Product.Application.Queries.GetAllProducts;
using Product.Application.Queries.GetProductById;
using Product.Application.Queries.GetProductsByCategory;
using Shared.Common.Responses;

namespace Product.API.Controllers;

/// <summary>
/// Product Controller — CQRS + MediatR ile ürün CRUD endpoint'leri.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Tüm ürünleri listeler (cache'li).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllProductsQuery());
        return Ok(ApiResponse<List<ProductDto>>.Ok(result));
    }

    /// <summary>
    /// ID'ye göre tekil ürün getirir.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery { Id = id });
        return Ok(ApiResponse<ProductDto>.Ok(result));
    }

    /// <summary>
    /// Yeni ürün ekler. JWT doğrulaması gerektirir.
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        return StatusCode(201, ApiResponse<ProductDto>.Created(result));
    }

    /// <summary>
    /// Ürün günceller. JWT doğrulaması gerektirir.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<ProductDto>.Ok(result, "Ürün güncellendi."));
    }

    /// <summary>
    /// Kategoriye göre ürün listeler — Raw T-SQL sorgusu kullanır.
    /// </summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(ApiResponse<List<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(string category)
    {
        var result = await _mediator.Send(new GetProductsByCategoryQuery { Category = category });
        return Ok(ApiResponse<List<ProductDto>>.Ok(result));
    }

    /// <summary>
    /// Ürün siler. Sadece Admin rolü erişebilir.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteProductCommand { Id = id });
        return Ok(ApiResponse<bool>.Ok(result, "Ürün silindi."));
    }
}
