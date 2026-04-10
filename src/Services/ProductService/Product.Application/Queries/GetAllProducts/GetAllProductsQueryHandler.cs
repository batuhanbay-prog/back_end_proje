using AutoMapper;
using MediatR;
using Product.Application.DTOs;
using Product.Application.Interfaces;
using Shared.Common.Constants;

namespace Product.Application.Queries.GetAllProducts;

/// <summary>
/// GetAllProductsQuery handler'ı.
/// Önce Redis'e bakar, yoksa veritabanından çeker ve cache'e yazar (Cache-Aside Pattern).
/// </summary>
public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;

    public GetAllProductsQueryHandler(IProductRepository repository, ICacheService cacheService, IMapper mapper)
    {
        _repository = repository;
        _cacheService = cacheService;
        _mapper = mapper;
    }

    public async Task<List<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        // 1. Redis'te var mı?
        var cachedProducts = await _cacheService.GetAsync<List<ProductDto>>(CacheKeys.AllProducts);
        if (cachedProducts != null)
            return cachedProducts;

        // 2. Veritabanından çek
        var products = await _repository.GetAllAsync();
        var productDtos = _mapper.Map<List<ProductDto>>(products);

        // 3. Redis'e kaydet (10 dakika)
        await _cacheService.SetAsync(CacheKeys.AllProducts, productDtos, TimeSpan.FromMinutes(10));

        return productDtos;
    }
}
