using AutoMapper;
using FluentAssertions;
using Moq;
using Product.Application.Commands.CreateProduct;
using Product.Application.DTOs;
using Product.Application.Interfaces;
using Shared.Common.Constants;
using Shared.Common.Events;

namespace Product.UnitTests;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _mockRepo;
    private readonly Mock<ICacheService> _mockCache;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IEventBus> _mockEventBus;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _mockRepo = new Mock<IProductRepository>();
        _mockCache = new Mock<ICacheService>();
        _mockMapper = new Mock<IMapper>();
        _mockEventBus = new Mock<IEventBus>();

        _mockMapper.Setup(m => m.Map<ProductDto>(It.IsAny<Product.Domain.Entities.Product>()))
            .Returns((Product.Domain.Entities.Product p) => new ProductDto
            {
                Id = p.Id, Name = p.Name, Price = p.Price, Stock = p.Stock
            });

        _handler = new CreateProductCommandHandler(
            _mockRepo.Object, _mockCache.Object, _mockMapper.Object, _mockEventBus.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldAddProductToRepository()
    {
        var command = new CreateProductCommand { Name = "Test Ürün", Price = 100, Stock = 10, Category = "Elektronik" };

        var result = await _handler.Handle(command, CancellationToken.None);

        _mockRepo.Verify(r => r.AddAsync(It.IsAny<Product.Domain.Entities.Product>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldInvalidateCache()
    {
        var command = new CreateProductCommand { Name = "Test Ürün", Price = 100, Stock = 10 };

        await _handler.Handle(command, CancellationToken.None);

        _mockCache.Verify(c => c.RemoveAsync(CacheKeys.AllProducts), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldPublishEvent()
    {
        var command = new CreateProductCommand { Name = "Test Ürün", Price = 150, Stock = 5 };

        await _handler.Handle(command, CancellationToken.None);

        _mockEventBus.Verify(e => e.PublishAsync(It.IsAny<ProductCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnProductDto()
    {
        var command = new CreateProductCommand { Name = "Test Ürün", Price = 99.99m, Stock = 20 };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Test Ürün");
        result.Price.Should().Be(99.99m);
    }
}
