using Log.Application.Interfaces;
using Log.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Common.Events;

namespace Log.Infrastructure.Consumers;

/// <summary>
/// RabbitMQ'dan ProductCreatedEvent consume eder ve LogDb'ye kayıt yazar.
/// Event akışı: Product Service → RabbitMQ → Log Service → LogDb
/// </summary>
public class ProductCreatedEventConsumer : IConsumer<ProductCreatedEvent>
{
    private readonly ILogRepository _logRepository;
    private readonly ILogger<ProductCreatedEventConsumer> _logger;

    public ProductCreatedEventConsumer(ILogRepository logRepository, ILogger<ProductCreatedEventConsumer> logger)
    {
        _logRepository = logRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation("[LogService] ProductCreatedEvent alındı. ProductId: {ProductId}, Name: {Name}",
            @event.ProductId, @event.ProductName);

        var logEntry = new LogEntry
        {
            ServiceName = "ProductService",
            LogLevel = "INFO",
            Message = $"Ürün oluşturuldu: {@event.ProductName} (Id: {@event.ProductId}) - Fiyat: {@event.Price}, Stok: {@event.Stock}",
            Timestamp = @event.CreatedDate,
            CorrelationId = context.CorrelationId?.ToString()
        };

        await _logRepository.AddAsync(logEntry);
    }
}
