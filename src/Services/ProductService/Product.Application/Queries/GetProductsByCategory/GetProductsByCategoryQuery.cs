using MediatR;
using Product.Application.DTOs;

namespace Product.Application.Queries.GetProductsByCategory;

/// <summary>
/// Kategoriye göre ürün listesi — Raw T-SQL (FromSqlRaw) kullanır.
/// </summary>
public class GetProductsByCategoryQuery : IRequest<List<ProductDto>>
{
    public string Category { get; set; } = string.Empty;
}
