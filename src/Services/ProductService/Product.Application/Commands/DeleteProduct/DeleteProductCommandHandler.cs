using MediatR;
using Product.Application.Interfaces;
using Shared.Common.Constants;
using Shared.Common.Exceptions;

namespace Product.Application.Commands.DeleteProduct;

/// <summary>
/// DeleteProductCommand handler'ı.
/// Ürünü siler, cache'i invalidate eder.
/// </summary>
public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _repository;
    private readonly ICacheService _cacheService;

    public DeleteProductCommandHandler(IProductRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException("Ürün", request.Id);

        await _repository.DeleteAsync(product);

        // Cache invalidation
        await _cacheService.RemoveAsync(CacheKeys.AllProducts);
        await _cacheService.RemoveAsync(CacheKeys.ProductById(request.Id));

        return true;
    }
}
