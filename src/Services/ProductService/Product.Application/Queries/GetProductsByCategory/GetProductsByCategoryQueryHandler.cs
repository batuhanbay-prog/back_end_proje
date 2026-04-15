using AutoMapper;
using MediatR;
using Product.Application.DTOs;
using Product.Application.Interfaces;

namespace Product.Application.Queries.GetProductsByCategory;

/// <summary>
/// GetProductsByCategoryQuery handler — repository'deki Raw T-SQL metodunu çağırır.
/// </summary>
public class GetProductsByCategoryQueryHandler : IRequestHandler<GetProductsByCategoryQuery, List<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public GetProductsByCategoryQueryHandler(IProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<ProductDto>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var products = await _repository.GetByCategoryAsync(request.Category);
        return _mapper.Map<List<ProductDto>>(products);
    }
}
