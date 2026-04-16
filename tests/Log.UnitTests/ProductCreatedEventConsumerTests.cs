using FluentAssertions;
using Log.Application.Interfaces;
using Log.Domain.Entities;
using Log.Infrastructure.Consumers;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Common.Events;

namespace Log.UnitTests;

public class ProductCreatedEventConsumerTests
{
    private readonly Mock<ILogRepository> _mockRepo;
    private readonly Mock<ILogger<ProductCreatedEventConsumer>> _mockLogger;
    private readonly ProductCreatedEventConsumer _consumer;

    public ProductCreatedEventConsumerTests()
    {
        _mockRepo = new Mock<ILogRepository>();
        _mockLogger = new Mock<ILogger<ProductCreatedEventConsumer>>();
        _consumer = new ProductCreatedEventConsumer(_mockRepo.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Consume_ValidEvent_ShouldSaveLogEntry()
    {
        var @event = new ProductCreatedEvent
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Test Ürün",
            Price = 99.99m,
            Stock = 10,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "system"
        };

        var mockContext = new Mock<ConsumeContext<ProductCreatedEvent>>();
        mockContext.Setup(c => c.Message).Returns(@event);
        mockContext.Setup(c => c.CorrelationId).Returns(Guid.NewGuid());

        await _consumer.Consume(mockContext.Object);

        _mockRepo.Verify(r => r.AddAsync(It.Is<LogEntry>(l =>
            l.ServiceName == "ProductService" &&
            l.LogLevel == "INFO" &&
            l.Message.Contains("Test Ürün"))), Times.Once);
    }

    [Fact]
    public async Task Consume_ValidEvent_ShouldCreateLogWithCorrectServiceName()
    {
        var @event = new ProductCreatedEvent
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Laptop",
            Price = 15000m,
            Stock = 5,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "admin"
        };

        var mockContext = new Mock<ConsumeContext<ProductCreatedEvent>>();
        mockContext.Setup(c => c.Message).Returns(@event);
        mockContext.Setup(c => c.CorrelationId).Returns((Guid?)null);

        LogEntry? capturedEntry = null;
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<LogEntry>()))
            .Callback<LogEntry>(entry => capturedEntry = entry);

        await _consumer.Consume(mockContext.Object);

        capturedEntry.Should().NotBeNull();
        capturedEntry!.ServiceName.Should().Be("ProductService");
        capturedEntry.LogLevel.Should().Be("INFO");
        capturedEntry.Message.Should().Contain("Laptop");
    }
}
