using MediatR;
using Product.Application.DTOs;

namespace Product.Application.Commands.CreateProduct;

/// <summary>
/// Ürün ekleme komutu.
/// </summary>
public class CreateProductCommand : IRequest<ProductDto>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
}
