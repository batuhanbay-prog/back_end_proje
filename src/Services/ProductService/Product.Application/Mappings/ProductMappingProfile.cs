using AutoMapper;
using Product.Application.DTOs;

namespace Product.Application.Mappings;

/// <summary>
/// AutoMapper profili — Entity ↔ DTO dönüşümü.
/// </summary>
public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product.Domain.Entities.Product, ProductDto>().ReverseMap();
    }
}
