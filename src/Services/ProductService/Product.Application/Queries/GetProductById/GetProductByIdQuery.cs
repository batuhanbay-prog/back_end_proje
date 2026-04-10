using MediatR;
using Product.Application.DTOs;

namespace Product.Application.Queries.GetProductById;

/// <summary>
/// Tekil ürün getir sorgusu.
/// </summary>
public class GetProductByIdQuery : IRequest<ProductDto>
{
    public Guid Id { get; set; }
}
