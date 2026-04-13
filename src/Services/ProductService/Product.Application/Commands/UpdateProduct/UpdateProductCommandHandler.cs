using AutoMapper;
using MediatR;
using Product.Application.DTOs;
using Product.Application.Interfaces;
using Shared.Common.Constants;
using Shared.Common.Events;
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
    private readonly IEventBus _eventBus;

    public UpdateProductCommandHandler(IProductRepository repository, ICacheService cacheService, IMapper mapper, IEventBus eventBus)
    {
        _repository = repository;
        _cacheService = cacheService;
        _mapper = mapper;
        _eventBus = eventBus;
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

        // ProductUpdatedEvent fırlat → RabbitMQ → Log Service consume eder
        await _eventBus.PublishAsync(new ProductUpdatedEvent
        {
            ProductId = product.Id,
            ProductName = product.Name,
            NewPrice = product.Price,
            NewStock = product.Stock,
            UpdatedDate = product.UpdatedDate ?? DateTime.UtcNow,
            UpdatedBy = "system"
        });

        return _mapper.Map<ProductDto>(product);
    }
}
