using AutoMapper;
using FluentAssertions;
using Moq;
using Product.Application.DTOs;
using Product.Application.Interfaces;
using Product.Application.Queries.GetAllProducts;
using Shared.Common.Constants;

namespace Product.UnitTests;

public class GetAllProductsQueryHandlerTests
{
    private readonly Mock<IProductRepository> _mockRepo;
    private readonly Mock<ICacheService> _mockCache;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetAllProductsQueryHandler _handler;

    public GetAllProductsQueryHandlerTests()
    {
        _mockRepo = new Mock<IProductRepository>();
        _mockCache = new Mock<ICacheService>();
        _mockMapper = new Mock<IMapper>();

        _handler = new GetAllProductsQueryHandler(_mockRepo.Object, _mockCache.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_CacheHit_ShouldNotCallRepository()
    {
        var cachedProducts = new List<ProductDto> { new ProductDto { Id = Guid.NewGuid(), Name = "Cached Ürün" } };
        _mockCache.Setup(c => c.GetAsync<List<ProductDto>>(CacheKeys.AllProducts))
            .ReturnsAsync(cachedProducts);

        var result = await _handler.Handle(new GetAllProductsQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        _mockRepo.Verify(r => r.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_CacheMiss_ShouldCallRepositoryAndSetCache()
    {
        var dbProducts = new List<Product.Domain.Entities.Product>
        {
            new Product.Domain.Entities.Product { Id = Guid.NewGuid(), Name = "DB Ürün", Price = 50 }
        };
        var dtoProducts = new List<ProductDto> { new ProductDto { Id = dbProducts[0].Id, Name = "DB Ürün" } };

        _mockCache.Setup(c => c.GetAsync<List<ProductDto>>(CacheKeys.AllProducts)).ReturnsAsync((List<ProductDto>?)null);
        _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(dbProducts);
        _mockMapper.Setup(m => m.Map<List<ProductDto>>(dbProducts)).Returns(dtoProducts);

        var result = await _handler.Handle(new GetAllProductsQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        _mockRepo.Verify(r => r.GetAllAsync(), Times.Once);
        _mockCache.Verify(c => c.SetAsync(CacheKeys.AllProducts, It.IsAny<List<ProductDto>>(), It.IsAny<TimeSpan?>()), Times.Once);
    }
}
