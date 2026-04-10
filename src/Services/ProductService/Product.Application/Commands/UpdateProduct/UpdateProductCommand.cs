using MediatR;
using Product.Application.DTOs;

namespace Product.Application.Commands.UpdateProduct;

/// <summary>
/// Ürün güncelleme komutu.
/// </summary>
public class UpdateProductCommand : IRequest<ProductDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
}
