using MediatR;

namespace Product.Application.Commands.DeleteProduct;

/// <summary>
/// Ürün silme komutu.
/// </summary>
public class DeleteProductCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
