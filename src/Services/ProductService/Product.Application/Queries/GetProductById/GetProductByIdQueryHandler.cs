using AutoMapper;
using MediatR;
using Product.Application.DTOs;
using Product.Application.Interfaces;
using Shared.Common.Constants;
using Shared.Common.Exceptions;

namespace Product.Application.Queries.GetProductById;

/// <summary>
/// GetProductByIdQuery handler'ı.
/// Önce Redis'e bakar, yoksa veritabanından çeker (Cache-Aside Pattern).
/// </summary>
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public GetProductByIdQueryHandler(IProductRepository repository, ICacheService cacheService, IMapper mapper)
    {
        _repository = repository;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        // 1. Redis'te var mı?
        var cacheKey = CacheKeys.ProductById(request.Id);
        var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey);
        if (cachedProduct != null)
            return cachedProduct;

        // 2. Veritabanından çek
        var product = await _repository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException("Ürün", request.Id);

        var productDto = _mapper.Map<ProductDto>(product);

        // 3. Redis'e kaydet (10 dakika)
        await _cacheService.SetAsync(cacheKey, productDto, TimeSpan.FromMinutes(10));

        return productDto;
    }
}
