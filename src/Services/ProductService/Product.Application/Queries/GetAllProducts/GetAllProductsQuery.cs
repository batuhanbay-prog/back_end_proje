using MediatR;
using Product.Application.DTOs;

namespace Product.Application.Queries.GetAllProducts;

/// <summary>
/// Tüm ürünleri getir sorgusu.
/// </summary>
public class GetAllProductsQuery : IRequest<List<ProductDto>>
{
}
