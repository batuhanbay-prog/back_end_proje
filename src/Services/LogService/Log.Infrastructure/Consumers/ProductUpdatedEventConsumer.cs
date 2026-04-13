using Log.Application.Interfaces;
using Log.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Common.Events;

namespace Log.Infrastructure.Consumers;

/// <summary>
/// RabbitMQ'dan ProductUpdatedEvent consume eder ve LogDb'ye kayıt yazar.
/// Event akışı: Product Service → RabbitMQ → Log Service → LogDb
/// </summary>
public class ProductUpdatedEventConsumer : IConsumer<ProductUpdatedEvent>
{
    private readonly ILogRepository _logRepository;
    private readonly ILogger<ProductUpdatedEventConsumer> _logger;

    public ProductUpdatedEventConsumer(ILogRepository logRepository, ILogger<ProductUpdatedEventConsumer> logger)
    {
        _logRepository = logRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductUpdatedEvent> context)
    {
        var @event = context.Message;

        _logger.LogInformation("[LogService] ProductUpdatedEvent alındı. ProductId: {ProductId}, Name: {Name}",
            @event.ProductId, @event.ProductName);

        var logEntry = new LogEntry
        {
            ServiceName = "ProductService",
            LogLevel = "INFO",
            Message = $"Ürün güncellendi: {@event.ProductName} (Id: {@event.ProductId}) - Yeni Fiyat: {@event.NewPrice}, Yeni Stok: {@event.NewStock}",
            Timestamp = @event.UpdatedDate,
            CorrelationId = context.CorrelationId?.ToString()
        };

        await _logRepository.AddAsync(logEntry);
    }
}
