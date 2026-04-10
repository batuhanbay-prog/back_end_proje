using AutoMapper;
using MediatR;
using Product.Application.DTOs;
using Product.Application.Interfaces;
using Shared.Common.Constants;
using Shared.Common.Exceptions;

namespace Product.Application.Commands.UpdateProduct;

/// <summary>
/// UpdateProductCommand handler'ı.
/// Ürünü günceller, cache'i invalidate eder.
/// </summary>
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(IProductRepository repository, ICacheService cacheService, IMapper mapper)
    {
        _repository = repository;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException("Ürün", request.Id);

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.Category = request.Category;
        product.UpdatedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(product);

        // Cache invalidation
        await _cacheService.RemoveAsync(CacheKeys.AllProducts);
        await _cacheService.RemoveAsync(CacheKeys.ProductById(product.Id));

        // Event fırlatma — ADIM 7'de MassTransit ile eklenecek

        return _mapper.Map<ProductDto>(product);
    }
}
