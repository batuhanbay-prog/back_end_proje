using AutoMapper;
using MediatR;
using Product.Application.DTOs;
using Product.Application.Interfaces;
using Shared.Common.Constants;
using Shared.Common.Events;

namespace Product.Application.Commands.CreateProduct;

/// <summary>
/// CreateProductCommand handler'ı.
/// Veritabanına ürün ekler, cache'i invalidate eder.
/// Event fırlatma ADIM 7'de eklenecek.
/// </summary>
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly IEventBus _eventBus;

    public CreateProductCommandHandler(IProductRepository repository, ICacheService cacheService, IMapper mapper, IEventBus eventBus)
    {
        _repository = repository;
        _cacheService = cacheService;
        _mapper = mapper;
        _eventBus = eventBus;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product.Domain.Entities.Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            Category = request.Category
        };

        // 1. Veritabanına ekle
        await _repository.AddAsync(product);

        // 2. Cache'i invalidate et
        await _cacheService.RemoveAsync(CacheKeys.AllProducts);

        // 3. ProductCreatedEvent fırlat → RabbitMQ → Log Service consume eder
        await _eventBus.PublishAsync(new ProductCreatedEvent
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Price = product.Price,
            Stock = product.Stock,
            CreatedDate = product.CreatedDate,
            CreatedBy = "system"
        });

        return _mapper.Map<ProductDto>(product);
    }
}
